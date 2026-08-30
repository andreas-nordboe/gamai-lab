using GamAILab.Shared.Models.AICodeEvaluation.Hints;

namespace GamAILab.Shared.Models.Game.DTOs;

public class CodeTaskLearnerProgress
{
    public int Attempts { get; set; }
    public int HintsUsed { get; set; }
    public List<AICodeHintChatLog> ChatLogs { get; set; } = [];
}