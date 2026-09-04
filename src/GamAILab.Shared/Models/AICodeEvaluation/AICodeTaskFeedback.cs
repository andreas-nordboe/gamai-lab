using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AICodeEvaluation;

public class AICodeTaskFeedback
{
    [Key]
    public int Id { get; set; }

    public int CodeSubmissionId { get; set; }
    [JsonIgnore]
    public CodeSubmission.CodeSubmission CodeSubmission { get; set; } = null!;
    public CodeTaskOutcome TaskOutcome { get; set; }
    public string? HintMessage  { get; set; }
    public string CodeTaskExecutionEvidence { get; set; } = "[]"; // JSON probalby works here..
    public string LLMModelUsed { get; set; } = string.Empty;
    public string Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public long GeneationTimeInMs { get; set; } 
    
}