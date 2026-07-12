namespace GamAILab.Shared.Models.AICodeEvaluation;

// DTO that is separated from database entity (from LLM output!) 
public sealed class AICodeEvaluationPlanOutput
{
    public required List<string> Criteria { get; init; }
    public required List<string> CommonMistakes { get; init; }
    public required string FeedbackInstructions { get; init; }
    public required string Language { get; init; }
    public required IReadOnlyList<AICodeEvaluationTest> Tests { get; init; }
}