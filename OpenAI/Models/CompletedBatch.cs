using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WordList.Common.OpenAI.Models;

public class CompletedBatch
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("custom_id")]
    public required string CustomId { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("response")]
    public required CompletedBatchResponse Response { get; set; }
}

public class CompletedBatchResponse
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("request_id")]
    public required string RequestId { get; set; }

    [JsonPropertyName("body")]
    public required CompletedBatchResponseBody Body { get; set; }
}

public class CompletedBatchResponseBody
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("object")]
    public required string Object { get; set; }

    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("background")]
    public bool IsBackground { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("output")]
    public required CompletedBatchResponseBodyOutput Output { get; set; }

    [JsonPropertyName("service_tier")]
    public required string ServiceTier { get; set; }

    [JsonPropertyName("usage")]
    public required CompletedBatchResponseBodyUsage Usage { get; set; }
}

public class CompletedBatchResponseBodyOutput
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("content")]
    public required CompletedBatchResponseBodyOutputContent[] Content { get; set; } = [];

    [JsonPropertyName("role")]
    public required string Role { get; set; }
}

public class CompletedBatchResponseBodyOutputContent
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class CompletedBatchResponseBodyUsage
{
    [JsonPropertyName("input_tokens")]
    public required long InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public required long OutputTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public required long TotalTokens { get; set; }
}