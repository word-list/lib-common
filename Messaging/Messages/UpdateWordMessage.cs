namespace WordList.Common.Messaging.Messages;

public class UpdateWordMessage
{
    public required string Word { get; set; }
    public int Offensiveness { get; set; }
    public int Commonness { get; set; }
    public int Sentiment { get; set; }
    public string[] WordTypes { get; set; } = [];
}