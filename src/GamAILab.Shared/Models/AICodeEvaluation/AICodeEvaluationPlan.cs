using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.AICodeEvaluation;

public sealed class AICodeEvaluationPlan
{
    [Key]
    public string Id { get; set; } = null!;
    public required CodeTask CodeTask { get; set; } // TODO Possibly change this to code task id int and add version attribute as well
    public required List<string> Criteria { get; set; }
    public required List<string> CommonMistakes { get; set; }
    public required string FeedbackInstructions { get; set; }
    public required string ModelUsed { get; set; }
    public required string Language { get; set; }
    public string PromptVersion { get; set; }
    public string SchemaVersion { get; set; }
    public required IReadOnlyList<AICodeEvaluationTest> Tests { get; set; }
    public DateTimeOffset InitiatedAt { get; set; }
    public required TimeSpan PlanningDuration { get; set; }
}