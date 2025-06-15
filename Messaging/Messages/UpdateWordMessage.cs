namespace WordList.Common.Messaging.Messages;

public class UpdateWordMessage
{
    public required string Word { get; set; }
    public int Offensiveness { get; set; }
    public int Commonness { get; set; }
    public int Sentiment { get; set; }
    public string[] WordTypes { get; set; } = [];

    public int Formality { get; set; }
    public int CulturalSensitivity { get; set; }
    public int Figurativeness { get; set; }
    public int Complexity { get; set; }
    public int Political { get; set; }
}