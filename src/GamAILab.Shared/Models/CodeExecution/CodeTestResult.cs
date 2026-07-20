using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.CodeExecution;

public sealed class CodeTestResult
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("passed")]
    public bool Passed { get; init; }
    [JsonPropertyName("expectedResult")]
    public JsonElement? ExpectedResult { get; init; }
    [JsonPropertyName("actualResult")]
    public JsonElement? ActualOutput { get; init; }
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}