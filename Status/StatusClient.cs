using Amazon.DynamoDBv2.DataModel;
using WordList.Common.Status.Models;

namespace WordList.Common.Status;

public class StatusClient
{
    private static readonly DynamoDBContext s_dynamoDb = new DynamoDBContextBuilder().Build();

    public string TableName { get; init; }

    public string StatusId { get; init; }

    private SaveConfig SaveConfig { get; init; }

    private LoadConfig LoadConfig { get; init; }

    public StatusClient(string statusId)
    {
        TableName = Environment.GetEnvironmentVariable("SOURCE_UPDATE_STATUS_TABLE_NAME")
            ?? throw new InvalidOperationException("DYNAMODB_TABLE_NAME environment variable is not set.");

        if (string.IsNullOrWhiteSpace(TableName))
            throw new InvalidOperationException("SOURCE_UPDATE_STATUS_TABLE_NAME environment variable is not set or is empty.");

        StatusId = statusId;

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
            Id = StatusId,
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

    public async Task UpdateTotalWordsAsync(int totalWords)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.TotalWords = totalWords;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task UpdateTotalChunksAsync(int totalChunks)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.TotalChunks = totalChunks;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task UpdateTotalBatchesAsync(int totalBatches)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.TotalBatches = totalBatches;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task IncreaseTotalBatchesAsync(int totalBatches)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.TotalBatches += totalBatches;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task IncreaseProcessedBatchesAsync(int processedBatches)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.ProcessedBatches += processedBatches;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task UpdateStatusAsync(SourceStatus status)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        // Only update the status if the new status has a higher priority.
        if (SourceStatus.FromText(statusUpdate.Status).Priority < status.Priority)
        {
            statusUpdate.Status = status.Text;
            statusUpdate.LastUpdated = DateTime.UtcNow;
            await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
        }
    }

    public async Task IncreaseProcessedWordsAsync(int processedWords)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.ProcessedWords += processedWords;
        if (statusUpdate.ProcessedWords >= statusUpdate.TotalWords)
        {
            statusUpdate.ProcessedWords = statusUpdate.TotalWords;
            statusUpdate.Status = SourceStatus.COMPLETE.Text;
        }
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task IncreaseProcessedChunksAsync(int processedChunks)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<Models.SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.ProcessedChunks += processedChunks;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task IncreaseTotalWordsAsync(int totalWordsDelta)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<Models.SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.TotalWords += totalWordsDelta;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }

    public async Task IncreaseRetriedWordsAsync(int retriedWords)
    {
        var statusUpdate = await s_dynamoDb.LoadAsync<Models.SourceUpdateStatus>(StatusId, LoadConfig).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Status with ID {StatusId} not found.");

        statusUpdate.RetriedWords += retriedWords;
        statusUpdate.LastUpdated = DateTime.UtcNow;
        await s_dynamoDb.SaveAsync(statusUpdate, SaveConfig).ConfigureAwait(false);
    }
}