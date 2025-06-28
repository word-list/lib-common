namespace WordList.Common.Words.Models;

public class AttributeRange
{
    public int Min { get; set; }
    public int Max { get; set; }

    public AttributeRange(int min, int max)
    {
        Min = min;
        Max = max;
    }
}