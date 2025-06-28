namespace WordList.Common.Words.Models;

public readonly record struct UpsertWordsResult
{
    public int ModifiedWordsCount { get; init; }
    public int ModifiedWordTypesCount { get; init; }
    public int ModifiedWordWordTypesCount { get; init; }
}