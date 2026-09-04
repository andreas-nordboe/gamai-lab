using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.AICodeEvaluation.Hints;

public class AICodeHintChatLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int CodeTaskId { get; set; }
    public AICodeHintChatLogRole ChatLogRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? HintLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}