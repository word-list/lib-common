
using System.Text.Json.Serialization.Metadata;
using WordList.Common.Messaging.Messages;

namespace WordList.Common.Messaging;

public static class MessageQueues
{
    private static MessageQueue<UpdateWordMessage>? s_updateWords;
    public static MessageQueue<UpdateWordMessage> UpdateWords
        => s_updateWords ??= MessageQueue<UpdateWordMessage>.FromEnvironmentVariable("UPDATE_WORDS_QUEUE_URL", MessageSerializerContext.Default.UpdateWordMessage);

    private static MessageQueue<UpdateBatchMessage>? s_updateBatch;
    public static MessageQueue<UpdateBatchMessage> UpdateBatch
        => s_updateBatch ??= MessageQueue<UpdateBatchMessage>.FromEnvironmentVariable("UPDATE_BATCH_QUEUE_URL", MessageSerializerContext.Default.UpdateBatchMessage);

    private static MessageQueue<ProcessSourceChunkMessage>? s_processSourceChunk;
    public static MessageQueue<ProcessSourceChunkMessage> ProcessSourceChunk
        => s_processSourceChunk ??= MessageQueue<ProcessSourceChunkMessage>.FromEnvironmentVariable("PROCESS_SOURCE_CHUNK_QUEUE_URL", MessageSerializerContext.Default.ProcessSourceChunkMessage);

    private static MessageQueue<QueryWordMessage>? s_queryWords;
    public static MessageQueue<QueryWordMessage> QueryWords
        => s_queryWords ??= MessageQueue<QueryWordMessage>.FromEnvironmentVariable("QUERY_WORDS_QUEUE_URL", MessageSerializerContext.Default.QueryWordMessage);

    private static MessageQueue<UploadSourceChunksMessage>? s_uploadSourceChunks;
    public static MessageQueue<UploadSourceChunksMessage> UploadSourceChunks
        => s_uploadSourceChunks ??= MessageQueue<UploadSourceChunksMessage>.FromEnvironmentVariable("UPLOAD_SOURCE_CHUNKS_QUEUE_URL", MessageSerializerContext.Default.UploadSourceChunksMessage);

}