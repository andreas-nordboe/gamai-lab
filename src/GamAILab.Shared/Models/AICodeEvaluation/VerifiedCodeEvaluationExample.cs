namespace GamAILab.Shared.Models.AICodeEvaluation;

public class VerifiedCodeEvaluationExample
{
    public int CodeTaskId { get; set; }
    public string CodeSubmission { get; set; } = string.Empty;
    public string CodeExecutionEvidence { get; set; } = string.Empty;
    public string PreviousFeedback { get; set; } = string.Empty;
}