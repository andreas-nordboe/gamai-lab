using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.AICodeEvaluation;

public sealed class AICodeEvaluationPlan
{
    [Key]
    public string Id { get; set; } = null!;
    public required int CodeTaskId { get; set; }
    public required int CodeTaskVersion { get; set; }
    public required List<string> Criteria { get; set; }
    public required List<string> CommonMistakes { get; set; }
    public required string FeedbackInstructions { get; set; }
    public required string ModelUsed { get; set; }
    public required string Language { get; set; }
    public string PromptVersion { get; set; }
    public string SchemaVersion { get; set; }
    public required List<AICodeEvaluationTest> Tests { get; set; } = [];
    public DateTimeOffset InitiatedAt { get; set; }
    public required TimeSpan PlanningDuration { get; set; }
}