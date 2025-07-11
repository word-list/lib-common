using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using Npgsql;
using WordList.Common.Words.Models;

namespace WordList.Common.Words;

public class WordDb
{
    private readonly NpgsqlDataSource _dataSource;

    public WordDb()
    {
        var envConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrEmpty(envConnectionString))
            throw new InvalidOperationException("DB_CONNECTION_STRING must be set");

        // Convert from a CockroachDB connection string
        var connectionStringUri = new Uri(envConnectionString);

        var userInfo = Uri.UnescapeDataString(connectionStringUri.UserInfo).Split(':');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connectionStringUri.Host,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = connectionStringUri.LocalPath.Trim('/'),
            Port = connectionStringUri.Port,
            Timeout = 120,
            SslMode = SslMode.VerifyFull,
            SearchPath = "public"
        };

        _dataSource = NpgsqlDataSource.Create(builder.ToString());
    }

    public async IAsyncEnumerable<string> GetExistingWordsAsync(params string[] words)
    {
        await using var conn = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("SELECT text FROM words WHERE text = ANY(@words)", conn);
        cmd.Parameters.AddWithValue("words", words);

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            yield return reader.GetString(0);
        }
    }

    private async Task<(int modifiedWordTypeCount, int modifiedWordWordTypeCount)>
        UpsertWordTypesAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, Word[] words)
    {
        var wordTypes = words.SelectMany(w => w.WordTypes).Distinct().ToArray();
        var wordWordTypes =
            from w in words
            from wt in w.WordTypes
            select new { Word = w.Text, Type = wt };

        await using var typeCmd = new NpgsqlCommand(@"
            UPSERT INTO word_types (name) 
            SELECT * FROM UNNEST(@typeArray)", conn, transaction);

        typeCmd.Parameters.AddWithValue("typeArray", wordTypes);

        var modifiedWordTypeCount = await typeCmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        // Update word type memberships
        await using var wordWordTypeCmd = new NpgsqlCommand(@"
            UPSERT INTO word_word_types (word_text, word_type_name)
            SELECT * FROM UNNEST(@wordTextArray, @wordTypeNameArray)", conn, transaction);

        wordWordTypeCmd.Parameters.AddWithValue("wordTextArray", wordWordTypes.Select(wt => wt.Word).ToArray());
        wordWordTypeCmd.Parameters.AddWithValue("wordTypeNameArray", wordWordTypes.Select(wt => wt.Type).ToArray());

        var modifiedWordWordTypeCount = await wordWordTypeCmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        return (modifiedWordTypeCount, modifiedWordWordTypeCount);
    }

    public async Task<UpsertWordsResult> UpsertWordsAsync(params Word[] words)
    {
        await using var conn = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
        await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

        var attributes = await WordAttributes.GetAllAsync().ConfigureAwait(false);

        var attributeNames = string.Join(", ", attributes.Select(a => a.Name));
        var attributeVarNames = string.Join(", ", attributes.Select(a => $"@{a.Name}Array"));

        // Update words
        await using var cmd = new NpgsqlCommand(@$"
            UPSERT INTO words 
                (text, {attributeNames})
            SELECT * FROM UNNEST
                (@textArray, {attributeVarNames})", conn, transaction);

        cmd.Parameters.AddWithValue("textArray", words.Select(w => w.Text).ToArray());

        foreach (var attr in attributes)
        {
            cmd.Parameters.AddWithValue($"{attr.Name}Array",
                words.Select(w => w.Attributes.TryGetValue(attr.Name, out var value) ? value : 0).ToArray());
        }

        var modifiedWordsCount = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        var (modifiedWordTypesCount, modifiedWordWordTypesCount) = await UpsertWordTypesAsync(conn, transaction, words).ConfigureAwait(false);

        await transaction.CommitAsync().ConfigureAwait(false);

        return new UpsertWordsResult
        {
            ModifiedWordsCount = modifiedWordsCount,
            ModifiedWordTypesCount = modifiedWordTypesCount,
            ModifiedWordWordTypesCount = modifiedWordWordTypesCount
        };
    }

    public async Task<List<Word>> FindWordsAsync(
        string? text = null,
        string[]? wordTypes = null,
        Dictionary<string, AttributeRange>? attributeRanges = null,
        string? from = null,
        string? randomSeed = null,
        int randomCount = 0,
        int limit = 100)
    {
        await using var conn = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        await WordAttributes.LoadAsync().ConfigureAwait(false);
        var attributes = await WordAttributes.GetAllAsync().ConfigureAwait(false);

        var attributeNames = string.Join(", ", attributes.Select(a => a.Name));

        var queryBuilder = new StringBuilder($"SELECT text, {attributeNames} FROM words ");
        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrEmpty(from))
        {
            conditions.Add($"WHERE text > @from ");
            parameters.Add(new("from", from));
        }

        if (randomCount > 0)
        {
            if (randomSeed == null)
            {
                randomSeed = Guid.NewGuid().ToString();
            }
            queryBuilder = new StringBuilder($"SELECT text, {attributeNames} FROM ({queryBuilder} ");
        }

        if (attributeRanges != null && attributeRanges.Count > 0)
        {
            foreach (var attr in attributeRanges)
            {
                var attrName = attr.Key;
                var range = attr.Value;

                conditions.Add($"{attrName} BETWEEN @{attrName}Start AND @{attrName}End ");
                parameters.Add(new($"@{attrName}Start", range.Min));
                parameters.Add(new($"@{attrName}End", range.Max));
            }
        }

        if (conditions.Count > 0)
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", conditions));
        }

        if (randomCount > 0)
        {
            queryBuilder.Append(" ORDER BY fnv64(CONCAT(@randomSeed, text)) LIMIT @randomCount) AS subquery ");
            parameters.Add(new("randomSeed", randomSeed));
            parameters.Add(new("randomCount", randomCount));
        }

        queryBuilder.Append(" ORDER BY text ASC LIMIT @limit;");
        parameters.Add(new("limit", limit));

        await using var cmd = new NpgsqlCommand(queryBuilder.ToString(), conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        Console.WriteLine($"Executing query: {cmd.CommandText}");

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

        var words = new List<Word>();

        var ordinalAttributes = new Dictionary<int, string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            if (attributes.Any(a => a.Name == name))
                ordinalAttributes[i] = name;
        }

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var thisText = "";
            var thisAttributes = new Dictionary<string, int>();

            for (var ordinal = 0; ordinal < reader.FieldCount; ordinal++)
            {

                if (reader.IsDBNull(ordinal))
                    continue;

                var fieldName = reader.GetName(ordinal);
                switch (fieldName)
                {
                    case "text":
                        thisText = reader.GetString(ordinal); break;
                    default:
                        if (!ordinalAttributes.TryGetValue(ordinal, out var attrName))
                            continue;

                        thisAttributes[attrName] = reader.GetInt32(ordinal);
                        break;
                }
            }

            words.Add(new()
            {
                Text = thisText,
                WordTypes = Array.Empty<string>(),
                Attributes = thisAttributes
            });
        }

        return words;
    }
}