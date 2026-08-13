using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.CodeExecution;

public sealed class CodeExecutionRunnerResponse
{
    [JsonPropertyName("didComplete")]
    public bool DidComplete { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    [JsonPropertyName("standardError")]
    public string StandardError { get; set; } = string.Empty;
    [JsonPropertyName("fatalError")]
    public string? FatalError { get; set; }
    [JsonPropertyName("testOutputs")]
    public List<CodeTestResult> TestsOutputs { get; set; } = [];
}
