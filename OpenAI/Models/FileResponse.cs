using System.Text.Json.Serialization;

namespace WordList.Common.OpenAI.Models;

public class FileResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}