using System.Text.Json;

namespace GamAILab.Shared.Models.CodeExecution;

public sealed class CodeExecutionRunnerResponse
{
    public bool DidComplete { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public string? FatalError { get; set; }
    public List<CodeTestResult> TestsOutputs { get; init; } = [];
}
