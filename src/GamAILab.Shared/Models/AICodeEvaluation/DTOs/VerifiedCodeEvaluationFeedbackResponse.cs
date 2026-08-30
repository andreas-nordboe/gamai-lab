namespace GamAILab.Shared.Models.AICodeEvaluation.DTOs;

public class VerifiedCodeEvaluationFeedbackResponse
{
    public int CodeTaskId { get; set; }
    public string CodeSubmission { get; set; } = string.Empty;
    public string CodeExecutionEvidence { get; set; } = string.Empty;
    public string PreviousFeedback { get; set; } = string.Empty;
    public bool Approved { get; set; }
}