using System.Text.Json.Serialization;

namespace WordList.Common.OpenAI.Models;

public class FileStatus
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("object")]
    public required string Object { get; set; }

    [JsonPropertyName("bytes")]
    public required long Bytes { get; set; }

    [JsonPropertyName("createdAt")]
    public required long CreatedAt { get; set; }

    [JsonPropertyName("filename")]
    public required string Filename { get; set; }

    [JsonPropertyName("purpose")]
    public required string Purpose { get; set; }
}