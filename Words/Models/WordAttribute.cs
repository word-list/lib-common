using Amazon.DynamoDBv2.DataModel;

namespace WordList.Common.Words.Models;

[DynamoDBTable("word-attributes-table")]
public class WordAttribute
{
    [DynamoDBHashKey("name")]
    public required string Name { get; set; }

    [DynamoDBProperty("display")]
    public required string Display { get; set; }

    [DynamoDBProperty("description")]
    public required string Description { get; set; }

    [DynamoDBProperty("prompt")]
    public required string Prompt { get; set; }

    [DynamoDBProperty("min")]
    public int Min { get; set; }

    [DynamoDBProperty("max")]
    public int Max { get; set; }

    public string GetSubstitutedPrompt()
        => Prompt
            .Replace("$MIN", Min.ToString())
            .Replace("$MAX", Max.ToString());
}