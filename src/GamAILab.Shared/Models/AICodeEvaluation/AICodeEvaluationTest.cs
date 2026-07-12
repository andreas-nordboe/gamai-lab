namespace GamAILab.Shared.Models.AICodeEvaluation;

public sealed class AICodeEvaluationTest
{
    public required string Name { get; init; }
    public required string Input { get; init; }
    public required string ExpectedOutput { get; init; }
}