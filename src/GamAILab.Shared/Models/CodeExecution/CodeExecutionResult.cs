using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.CodeExecution;

public class CodeExecutionResult
{
    [Key] 
    public int Id { get; set; }
    public bool DidComplete { get; init; }
    public bool TimedOut { get; init; }
    public bool EveryTestPassed { get; init; } 
    // TODO add amount of tests or ids that passed
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public string? FatalError { get; init; }
    public TimeSpan ExecutionDuration { get; init; }
    public IReadOnlyList<CodeTestResult> CodeTests { get; init; } = [];
    // TODO add execution failure type: none, learner, timeout, platform
}   