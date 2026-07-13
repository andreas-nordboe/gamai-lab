namespace GamAILab.Shared.Models.AICodeEvaluation;

public sealed class AICodeEvaluationTest
{
    public required string Name { get; init; }
    public required string FunctionName { get; init; } // TODO extend this later to be varied types of inputs (not just functions)
    public required List<int> Arguments { get; set; }
    public required string ExpectedOutput { get; init; }
}