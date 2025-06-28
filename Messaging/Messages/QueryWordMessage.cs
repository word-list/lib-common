namespace WordList.Common.Messaging.Messages;

public class QueryWordMessage
{
    public required string Word { get; set; }
    public required string CorrelationId { get; set; }
}