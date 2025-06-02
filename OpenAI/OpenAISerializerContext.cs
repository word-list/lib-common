using System.Text.Json.Serialization;
using WordList.Common.OpenAI.Models;

namespace WordList.Common.OpenAI;

[JsonSerializable(typeof(BatchRequestItem))]
[JsonSerializable(typeof(ResponsesRequest))]
[JsonSerializable(typeof(FileResponse))]
[JsonSerializable(typeof(CreateBatchRequest))]
[JsonSerializable(typeof(BatchStatus))]
public partial class OpenAISerializerContext : JsonSerializerContext
{
}