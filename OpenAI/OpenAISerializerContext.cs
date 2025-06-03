using System.Text.Json.Serialization;
using WordList.Common.OpenAI.Models;

namespace WordList.Common.OpenAI;

[JsonSerializable(typeof(BatchRequestItem))]
[JsonSerializable(typeof(ResponsesRequest))]
[JsonSerializable(typeof(FileResponse))]
[JsonSerializable(typeof(CreateBatchRequest))]
[JsonSerializable(typeof(BatchStatus))]
[JsonSerializable(typeof(FileStatus))]
[JsonSerializable(typeof(CompletedBatch))]
[JsonSerializable(typeof(CompletedBatchResponse))]
[JsonSerializable(typeof(CompletedBatchResponseBody))]
[JsonSerializable(typeof(CompletedBatchResponseBodyOutput))]
[JsonSerializable(typeof(CompletedBatchResponseBodyOutputContent))]
[JsonSerializable(typeof(CompletedBatchResponseBodyUsage))]
public partial class OpenAISerializerContext : JsonSerializerContext
{
}