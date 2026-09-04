using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GamAILab.Shared.Models.AICodeEvaluation;

namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmission
{
    [Key]
    public int Id { get; set; } // CodeSubmissionId
    public string? UserId { get; set; }
    public int CodeTaskId  { get; set; }
    public string? Code { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DateTime SubmittedAt { get; set; }
    public AICodeTaskFeedback? AICodeTaskFeedback { get; set; }
    // TODO possibly add submit frequency here
}