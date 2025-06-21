namespace WordList.Common.Messaging.Messages;

public class UpdateWordMessage
{
    public required string Word { get; set; }
    public string[] WordTypes { get; set; } = [];

    public Dictionary<string, int> Attributes { get; set; } = [];
}