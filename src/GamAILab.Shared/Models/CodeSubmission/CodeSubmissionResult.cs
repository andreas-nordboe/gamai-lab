using System.ComponentModel.DataAnnotations;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.AIHallucinationChecker.DTOs;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.Game;
using GamAILab.Shared.Models.Game.DTOs;

namespace GamAILab.Shared.Models.CodeSubmission;

public class CodeSubmissionResult
{
    [Key]
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int AttemptNumber { get; set; }
    public CodeTask CodeTask { get; set; } = null!; // TODO mask internal fields from response (seen by client)
    public CodeExecutionResult CodeExecution { get; set; } = null!; 
    public AICodeTaskFeedbackDTO AIFeedback { get; set; } = null!;
    public TimeSpan ExecutionDuration { get; set; }
    public string SubmittedCode { get; set; } // Returns code back to client so it can be displayed on the frontend
    public HallucinationCheckResult HallucinationCheck { get; set; } = null!;
    public LearnerGameProgressRequest? GameProgress { get; set; } 
}