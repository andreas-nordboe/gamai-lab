namespace GamAILab.Shared.Models.AICodeEvaluation.DTOs;

public class CodeEvaluationStatus
{
    public int CodeTaskId { get; set; }
    public CodeEvaluationStep CodeEvaluationStep { get; set; }
    public string Message { get; set; }
}