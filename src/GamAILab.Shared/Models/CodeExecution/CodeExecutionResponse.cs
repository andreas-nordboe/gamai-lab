namespace GamAILab.Shared.Models.CodeExecution;

public class CodeExecutionResponse
{
    public string CodeOutput { get; set; } = string.Empty;
    public string CodeError { get; set; } = string.Empty;
    public bool DidComplete { get; set; }
    public bool TimedOut { get; set; }
    public TimeSpan ExecutionDurion { get; set; }
}