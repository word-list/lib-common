namespace WordList.Common.Words.Models;

public class Word
{
    public required string Text { get; set; }
    public string[] WordTypes { get; set; } = [];

    public Dictionary<string, int> Attributes { get; set; } = [];
}