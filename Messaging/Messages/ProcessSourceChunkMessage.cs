namespace WordList.Common.Messaging.Messages;

public class ProcessSourceChunkMessage
{
    public required string SourceId { get; set; }
    public required string CorrelationId { get; set; }
    public required string ChunkId { get; set; }
    public required string Key { get; set; }
}