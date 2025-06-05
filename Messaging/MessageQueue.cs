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
    private readonly static AmazonSQSClient s_sqs = new();
    public string TargetUrl { get; init; }
    public JsonTypeInfo<T> JsonTypeInfo { get; init; }


    public MessageQueue(string targetUrl, JsonTypeInfo<T> jsonTypeInfo)
    {
        TargetUrl = targetUrl;
        JsonTypeInfo = jsonTypeInfo;
    }

    public BatchedMessageSender<T> GetBatchSender<U>(ILogger logger)
        => new(s_sqs, TargetUrl, JsonTypeInfo, logger);

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