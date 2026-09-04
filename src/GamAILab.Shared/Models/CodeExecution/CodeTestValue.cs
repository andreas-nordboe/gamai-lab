using GamAILab.Shared.Models.AICodeEvaluation;

namespace GamAILab.Shared.Models.CodeExecution;

public sealed class CodeTestValue
{
    public required ExpectedResultType Type { get; set; }
    public required string Value { get; set; }
}