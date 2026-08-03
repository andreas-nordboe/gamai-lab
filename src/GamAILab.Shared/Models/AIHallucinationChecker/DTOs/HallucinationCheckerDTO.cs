namespace GamAILab.Shared.Models.AIHallucinationChecker.DTOs;

// for API responses
public class HallucinationCheckerDTO
{
    public int Id { get; set; }
    public HallucinationCheckerStatus Status { get; set; }
    public bool IsReliable => Status == HallucinationCheckerStatus.IsConsistent;
    public string Summary { get; set; } = string.Empty;
    public string ConflictedClaims { get; set; } = "[]"; // empty JSON for now like in CodeExecutionEvidence should work
    public string LLMModelUsed { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long GenerationTimeInMilliseconds { get; set; } 
}