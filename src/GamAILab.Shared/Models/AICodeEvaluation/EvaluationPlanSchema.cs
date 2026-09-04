namespace GamAILab.Shared.Models.AICodeEvaluation;

public class EvaluationPlanSchema
{
    public List<string> Criteria { get; set; }
    public List<string> CommonMistakes { get; set; }
    public List<string> GeneratedTests { get; set; }
    public string FeedbackInstructions { get; set; }
}