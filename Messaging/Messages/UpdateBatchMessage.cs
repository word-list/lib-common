namespace WordList.Common.Messaging.Messages;

public class UpdateBatchMessage
{
    public required string BatchId { get; set; }
    public required string CorrelationId { get; set; }
}