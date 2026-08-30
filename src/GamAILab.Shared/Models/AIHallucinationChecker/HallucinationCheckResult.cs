using System.Text.Json.Serialization;
using GamAILab.Shared.Models.AICodeEvaluation;

namespace GamAILab.Shared.Models.AIHallucinationChecker;

public class HallucinationCheckResult
{
    public int Id { get; set; }
    public int AICodeTaskFeedbackId { get; set; }
    [JsonIgnore] 
    public AICodeTaskFeedback AICodeTaskFeedback { get; set; } = null!;

    public HallucinationCheckerStatus Status { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string ConflictedClaims { get; set; } = "[]"; // empty JSON for now like in CodeExecutionEvidence should work
    public string LLMModelUsed { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long GenerationTimeInMilliseconds { get; set; }
    public double ConsistencyScore { get; set; }
    public int TotalCheckedClaims { get; set; }
}