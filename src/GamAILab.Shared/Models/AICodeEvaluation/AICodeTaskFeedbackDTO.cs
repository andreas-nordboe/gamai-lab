namespace GamAILab.Shared.Models.AICodeEvaluation;

public class AICodeTaskFeedbackDTO
{
    public int Id { get; set; }
    public CodeTaskOutcome TaskOutcome { get; set; }
    public string? HintMessage  { get; set; }
    public string CodeTaskExecutionEvidence { get; set; } = "[]"; // JSON probalby works here..
    public string LLMModelUsed { get; set; } = string.Empty;
    public string Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public long GeneationTimeInMs { get; set; } 
}