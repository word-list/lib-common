using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using WordList.Common.Json;
using WordList.Common.Logging;
using WordList.Common.Messaging.Messages;

namespace WordList.Common.Messaging;

public class MessageQueue<T>
{
    /* For callers who are only receiving from Lambda input (i.e. SQSEvent), a queue URL
     * is not required.  This allows us to store the name of the environment variable we'd
     * use and only actually use it if needed.
     */
    private string? _environmentVariable;
    private readonly static AmazonSQSClient s_sqs = new();
    public string TargetUrl { get; private set; }
    public JsonTypeInfo<T> JsonTypeInfo { get; init; }


    public MessageQueue(string targetUrl, JsonTypeInfo<T> jsonTypeInfo)
    {
        TargetUrl = targetUrl;
        JsonTypeInfo = jsonTypeInfo;
    }

    public static MessageQueue<U> FromEnvironmentVariable<U>(string name, JsonTypeInfo<U> jsonTypeInfo)
        => new("", jsonTypeInfo) { _environmentVariable = name };

    public async Task SendBatchedMessagesAsync(ILogger logger, IEnumerable<T> messages)
    {
        var sender = GetBatchSender(logger);
        sender.AddRange(messages);
        await sender.SendAllMessagesAsync().ConfigureAwait(false);
    }

    public BatchedMessageSender<T> GetBatchSender(ILogger logger)
    {
        if (string.IsNullOrEmpty(TargetUrl))
        {
            if (string.IsNullOrEmpty(_environmentVariable))
                throw new Exception("Cannot send to a queue created with empty target URL and no environment variable");

            TargetUrl = Environment.GetEnvironmentVariable(_environmentVariable) ?? throw new Exception($"{_environmentVariable} must be defined (cannot send to queue)");
        }

        return new(s_sqs, TargetUrl, JsonTypeInfo, logger);
    }

    public T[] Receive(SQSEvent input, ILogger log)
    {
        return input.Records.Select(record =>
            {
                try
                {
                    return JsonHelpers.Deserialize(record.Body, JsonTypeInfo);
                }
                catch (Exception ex)
                {
                    log.Warning($"Ignoring invalid message: {record.Body} ({ex.Message})");
                    return default;
                }
            })
            .OfType<T>()
            .ToArray();
    }
}

[JsonSerializable(typeof(UpdateBatchMessage))]
[JsonSerializable(typeof(UpdateWordMessage))]
[JsonSerializable(typeof(ProcessSourceChunkMessage))]
[JsonSerializable(typeof(QueryWordMessage))]
[JsonSerializable(typeof(UploadSourceChunksMessage))]
public partial class MessageSerializerContext : JsonSerializerContext
{

}