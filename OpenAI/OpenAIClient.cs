
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WordList.Common.OpenAI.Models;
using WordList.Common.Json;
using System.Text;
using Microsoft.VisualBasic;
using Amazon.Runtime.Internal.Util;

namespace WordList.Common.OpenAI;

public class OpenAIClient
{
    private HttpClient _http = new();

    public OpenAIClient(string apiKey)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _http.BaseAddress = new Uri("https://api.openai.com/v1/", UriKind.Absolute);
    }

    public async Task<FileResponse?> CreateFileAsync(string filename, BatchRequestItem[] requests)
    {
        var jsonlLines = requests.Select(request => JsonHelpers.Serialize(request, OpenAISerializerContext.Default.BatchRequestItem));
        var jsonlBytes = Encoding.UTF8.GetBytes(string.Join("\n", jsonlLines));

        return await CreateFileAsync(filename, "batch", jsonlBytes).ConfigureAwait(false);
    }

    public async Task<FileResponse?> CreateFileAsync(string filename, string purpose, byte[] content)
    {
        using var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var form = new MultipartFormDataContent
        {
            { new StringContent(purpose), "purpose" },
            { fileContent, "file", filename }
        };

        Console.WriteLine($"Attempting to upload file to OpenAI...");

        var response = await _http.PostAsync("files", form).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"OpenAI API Error: {response.StatusCode} - {error}");
        }

        Console.WriteLine($"Response received from OpenAI: {response.StatusCode}");

        return await response.Content.ReadFromJsonAsync(OpenAISerializerContext.Default.FileResponse).ConfigureAwait(false);
    }

    public async Task<FileStatus?> GetFileStatusAsync(string fileId)
        => await _http.GetFromJsonAsync($"files/{fileId}", OpenAISerializerContext.Default.FileStatus);

    public async Task<CompletedBatch[]> GetCompletedBatchFileContentAsync(string fileId)
    {
        var content = await _http.GetStreamAsync($"files/{fileId}/content").ConfigureAwait(false);
        var reader = new StreamReader(content);

        var results = new List<CompletedBatch>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(line)) continue;

            var batch = JsonHelpers.Deserialize(line, OpenAISerializerContext.Default.CompletedBatch);
            if (batch is null) throw new Exception("Invalid response");

            results.Add(batch);
        }

        return [.. results];
    }

    public async Task<BatchStatus?> CreateBatchAsync(string uploadedFileId, string endpoint, string completionWindow = "24h")
    {
        var request = new CreateBatchRequest
        {
            InputFileId = uploadedFileId,
            Endpoint = endpoint,
            CompletionWindow = completionWindow
        };

        var response = await _http.PostAsJsonAsync("batches", request, OpenAISerializerContext.Default.CreateBatchRequest).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"OpenAI API Error: {response.StatusCode} - {error}");
        }

        return await response.Content.ReadFromJsonAsync(OpenAISerializerContext.Default.BatchStatus).ConfigureAwait(false);
    }

    public async Task<BatchStatus?> GetBatchStatusAsync(string batchId)
        => await _http.GetFromJsonAsync($"batches/{batchId}", OpenAISerializerContext.Default.BatchStatus).ConfigureAwait(false);

}
