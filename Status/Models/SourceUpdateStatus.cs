using Amazon.DynamoDBv2.DataModel;

namespace WordList.Common.Status.Models;

public class SourceUpdateStatus
{
    [DynamoDBHashKey("id")]
    public required string Id { get; set; }

    [DynamoDBProperty("sourceId")]
    public required string SourceId { get; set; }

    [DynamoDBProperty("status")]
    public string Status { get; set; } = Models.SourceStatus.UNKNOWN.Text;

    [DynamoDBProperty("totalWords")]
    public int TotalWords { get; set; }

    [DynamoDBProperty("processedWords")]
    public int ProcessedWords { get; set; }

    [DynamoDBProperty("totalChunks")]
    public int TotalChunks { get; set; }

    [DynamoDBProperty("processedChunks")]
    public int ProcessedChunks { get; set; }

    [DynamoDBProperty("retriedWords")]
    public int RetriedWords { get; set; }

    [DynamoDBProperty("started")]
    public DateTime Started { get; set; }

    [DynamoDBProperty("totalBatches")]
    public int TotalBatches { get; set; }

    [DynamoDBProperty("processedBatches")]
    public int ProcessedBatches { get; set; }

    [DynamoDBProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
