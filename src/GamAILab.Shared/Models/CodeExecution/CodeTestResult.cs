using System.Text.Json;

namespace GamAILab.Shared.Models.CodeExecution;

public sealed class CodeTestResult
{
    public required string Name { get; init; }
    public bool Passed { get; init; }
    public string ExpectedOutput { get; init; }
    public JsonElement? ActualOutput { get; init; }
    public string? Error { get; init; }
}