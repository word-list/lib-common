using Amazon.DynamoDBv2.DataModel;
using WordList.Common.Status.Models;

namespace WordList.Common.Status;

public class StatusClient
{
    private static readonly DynamoDBContext s_dynamoDb = new DynamoDBContextBuilder().Build();

    public string TableName { get; init; }

    private SaveConfig SaveConfig { get; init; }

    private LoadConfig LoadConfig { get; init; }

    private StatusClient()
    {
        TableName = Environment.GetEnvironmentVariable("SOURCE_UPDATE_STATUS_TABLE_NAME")
                        ?? throw new InvalidOperationException("DYNAMODB_TABLE_NAME environment variable is not set.");

        if (string.IsNullOrWhiteSpace(TableName))
        {
            throw new InvalidOperationException("SOURCE_UPDATE_STATUS_TABLE_NAME environment variable is not set or is empty.");
        }

        SaveConfig = new()
        {
            OverrideTableName = TableName
        };

        LoadConfig = new()
        {
            OverrideTableName = TableName
        };
    }

    public async Task<string> CreateStatusAsync(string sourceId)
    {
        var status = new SourceUpdateStatus
        {
            Id = Guid.NewGuid().ToString(),
            SourceId = sourceId,
            Status = SourceStatus.CHUNKING.Text,
            TotalWords = 0,
            ProcessedWords = 0,
            TotalChunks = 0,
            ProcessedChunks = 0,
            RetriedWords = 0,
            Started = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        await s_dynamoDb.SaveAsync(status, SaveConfig).ConfigureAwait(false);
        return status.Id;
    }

    public async Task UpdateStatusTotalsAsync(string statusId, int totalWords, int totalChunks)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(statusId, LoadConfig).ConfigureAwait(false);
        if (statusUpdate == null)
        {
            throw new InvalidOperationException($"Status with ID {statusId} not found.");
        }

        statusUpdate.TotalWords = totalWords;
        statusUpdate.TotalChunks = totalChunks;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task UpdateStatusAsync(string statusId, SourceStatus status)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(statusId).ConfigureAwait(false);
        if (statusUpdate == null)
        {
            throw new InvalidOperationException($"Status for source ID {statusId} not found.");
        }

        // Only update the status if the new status has a higher priority.
        if (SourceStatus.FromText(statusUpdate.Status).Priority < status.Priority)
        {
            statusUpdate.Status = status.Text;
            statusUpdate.LastUpdated = DateTime.UtcNow;
            await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
        }
    }

    public async Task IncreaseProcessedWordsAsync(string statusId, int processedWords)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(statusId).ConfigureAwait(false);
        if (statusUpdate == null)
        {
            throw new InvalidOperationException($"Status for source ID {statusId} not found.");
        }

        statusUpdate.ProcessedWords += processedWords;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task IncreaseProcessedChunksAsync(string statusId, int processedChunks)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<Models.SourceUpdateStatus>(statusId, LoadConfig).ConfigureAwait(false);
        if (statusUpdate == null)
        {
            throw new InvalidOperationException($"Status for source ID {statusId} not found.");
        }

        statusUpdate.ProcessedChunks += processedChunks;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }
}