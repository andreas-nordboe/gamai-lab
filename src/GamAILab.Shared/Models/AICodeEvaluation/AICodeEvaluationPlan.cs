using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.AICodeEvaluation;

public class AICodeEvaluationPlan
{
    [Key]
    public string Id { get; set; }
    public CodeTask CodeTask { get; set; }
    public List<string> Criteria { get; set; }
    public List<string> CommonMistakes { get; set; }
    public List<string> GeneratedTests { get; set; } // JSON would probably work here
    public string FeedbackInstructions { get; set; }
    public string ModelUsed { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime PlanningDuration { get; set; }
}