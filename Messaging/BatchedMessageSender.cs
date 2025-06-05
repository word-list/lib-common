
using System.Text.Json.Serialization.Metadata;
using Amazon.SQS;
using Amazon.SQS.Model;
using WordList.Common.Json;
using WordList.Common.Logging;

namespace WordList.Common.Messaging;

public class BatchedMessageSender<T> : IDisposable
{
    private AmazonSQSClient _sqs;
    private SemaphoreSlim _messageSendingLimiter = new(4);
    private ILogger _log;

    private List<T> _messages = new();

    public string TargetQueueUrl { get; init; }
    public JsonTypeInfo<T> JsonTypeInfo { get; init; }

    public int TryCount { get; init; } = 3;
    public int RetryDelay { get; init; } = 150;
    public int RetryBackoff { get; init; } = 100;


    public void Add(T message) => _messages.Add(message);
    public void AddRange(IEnumerable<T> messages) => _messages.AddRange(messages);

    private string? TrySerializeMessage(T message)
    {
        try
        {
            return JsonHelpers.Serialize(message, JsonTypeInfo);
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to serialize message with error: {ex.Message}");
            return null;
        }
    }

    public BatchedMessageSender(AmazonSQSClient sqs, string targetQueueUrl, JsonTypeInfo<T> jsonTypeInfo, ILogger logger)
    {
        TargetQueueUrl = targetQueueUrl;
        JsonTypeInfo = jsonTypeInfo;
        _log = logger;
        _sqs = sqs;
    }

    private async Task SendMessageBatchAsync(T[] messages)
    {
        var correlationId = Guid.NewGuid().ToString();
        var log = _log.WithPrefix($"[corr {correlationId}]");

        log.Info($"Waiting to send {messages.Length} message(s) to {TargetQueueUrl}");
        await _messageSendingLimiter.WaitAsync().ConfigureAwait(false);

        try
        {
            log.Info($"Sending {messages.Length} message(s)");

            var entryDict = messages
                .Select(TrySerializeMessage)
                .OfType<string>()
                .Select(text => new SendMessageBatchRequestEntry(
                    Guid.NewGuid().ToString(),
                    text
                ))
                .ToDictionary(entry => entry.Id);

            for (var tryNumber = 1; tryNumber <= TryCount && entryDict.Count > 0; tryNumber++)
            {
                var batchRequest = new SendMessageBatchRequest(TargetQueueUrl, [.. entryDict.Values]);

                if (tryNumber > 1) await Task.Delay(RetryDelay + tryNumber * RetryBackoff);

                log.Info($"Try number {tryNumber}: Sending batch of {batchRequest.Entries.Count} message(s)");
                try
                {
                    var response = await _sqs.SendMessageBatchAsync(batchRequest).ConfigureAwait(false);
                    foreach (var message in response.Successful)
                    {
                        entryDict.Remove(message.Id);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to send message batch of {batchRequest.Entries.Count} message(s): {ex.Message}");
                }
            }

            if (entryDict.Count > 0)
            {
                _log.Error($"Failed to send message batch of {entryDict.Count} message(s) after {TryCount} attempt(s)");
            }
        }
        finally
        {
            _messageSendingLimiter.Release();
        }
    }

    public async Task SendAllMessagesAsync()
    {
        var tasks = _messages.Chunk(10).Select(SendMessageBatchAsync);

        _log.Info($"Waiting for {tasks.Count()} message batches to send");
        await Task.WhenAll(tasks).ConfigureAwait(false);
        _log.Info($"Finished waiting for {tasks.Count()} message batches to send");

        _messages.Clear();
    }

    public void Clear() => _messages.Clear();

    public void Dispose()
    {
        if (_messages.Count > 0)
        {
            throw new Exception($"Message batch sender is being disposed with {_messages.Count} unsent messages.  SendAllMessagesAsync or Clear must be called.");
        }
    }
}