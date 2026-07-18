namespace GamAILab.Shared.Models.CodeExecution;

// Separate model/DTO for code single executions (not part of code submission and evaluation pipeline)
public class CodeExecutionRequest
{
    public string Code { get; set; } = string.Empty;
}