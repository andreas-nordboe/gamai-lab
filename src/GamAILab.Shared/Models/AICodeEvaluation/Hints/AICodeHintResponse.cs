namespace GamAILab.Shared.Models.AICodeEvaluation.Hints;

public class AICodeHintResponse
{
    public string Message { get; set; } = string.Empty;
    public int HintLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}   