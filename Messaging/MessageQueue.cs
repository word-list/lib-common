
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Amazon.SQS;
using Amazon.SQS.Model;
using WordList.Common.Logging;
using WordList.Common.Messaging.Messages;

namespace WordList.Common.Messaging;

public class MessageQueue<T>
{
    private readonly static AmazonSQSClient s_sqs = new();
    public string TargetUrl { get; init; }
    public JsonTypeInfo<T> JsonTypeInfo { get; init; }


    protected MessageQueue(string targetUrl, JsonTypeInfo<T> jsonTypeInfo)
    {
        TargetUrl = targetUrl;
        JsonTypeInfo = jsonTypeInfo;
    }

    protected static MessageQueue<U> GetQueueFromEnvironmentVariable<U>(string env, JsonTypeInfo<U> jsonTypeInfo)
    {
        var targetUrl = Environment.GetEnvironmentVariable(env)
            ?? throw new Exception($"{env} must be defined");

        return new(targetUrl, jsonTypeInfo);
    }

    private static MessageQueue<UpdateWordMessage>? s_updateWords;
    public static MessageQueue<UpdateWordMessage> UpdateWords
        => s_updateWords ??= GetQueueFromEnvironmentVariable("UPDATE_WORDS_QUEUE_URL", MessageSerializerContext.Default.UpdateWordMessage);

    private static MessageQueue<UpdateBatchMessage>? s_updateBatch;
    public static MessageQueue<UpdateBatchMessage> UpdateBatch
        => s_updateBatch ??= GetQueueFromEnvironmentVariable("UPDATE_BATCH_QUEUE_URL", MessageSerializerContext.Default.UpdateBatchMessage);

    private static MessageQueue<ProcessSourceChunkMessage>? s_processSourceChunk;
    public static MessageQueue<ProcessSourceChunkMessage> ProcessSourceChunk
        => s_processSourceChunk ??= GetQueueFromEnvironmentVariable("PROCESS_SOURCE_CHUNK_QUEUE_URL", MessageSerializerContext.Default.ProcessSourceChunkMessage);

    private static MessageQueue<QueryWordMessage>? s_queryWords;
    public static MessageQueue<QueryWordMessage> QueryWords
        => s_queryWords ??= GetQueueFromEnvironmentVariable("QUERY_WORDS_QUEUE_URL", MessageSerializerContext.Default.QueryWordMessage);

    private static MessageQueue<UploadSourceChunksMessage>? s_uploadSourceChunks;
    public static MessageQueue<UploadSourceChunksMessage> UploadSourceChunks
        => s_uploadSourceChunks ??= GetQueueFromEnvironmentVariable("UPLOAD_SOURCE_CHUNKS_QUEUE_URL", MessageSerializerContext.Default.UploadSourceChunksMessage);

    public BatchedMessageSender<T> GetBatchSender<U>(ILogger logger)
        => new(s_sqs, TargetUrl, JsonTypeInfo, logger);
}

[JsonSerializable(typeof(UpdateBatchMessage))]
[JsonSerializable(typeof(UpdateWordMessage))]
[JsonSerializable(typeof(ProcessSourceChunkMessage))]
[JsonSerializable(typeof(QueryWordMessage))]
[JsonSerializable(typeof(UploadSourceChunksMessage))]
public partial class MessageSerializerContext : JsonSerializerContext
{

}