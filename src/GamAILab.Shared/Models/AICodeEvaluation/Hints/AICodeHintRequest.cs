namespace GamAILab.Shared.Models.AICodeEvaluation.Hints;

public class AICodeHintRequest
{
    public int CodeTaskId { get; set; }
    public string LearnerCode { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string? LastCodeExecutionOutcome { get; set; } = string.Empty;
    public List<AICodeHintChatLog> ChatLogs { get; set; } = [];
}