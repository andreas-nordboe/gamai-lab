using System.ComponentModel.DataAnnotations;
using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.Shared.Models.AIPersonaSimulation;

public class AIPersonaSimulationResult
{
    [Key]
    public int Id { get; set; }
    public AIPersona Persona { get; set; }
    public AIPersonaCodeResponse CodeAttempt { get; init; }
    public CodeSubmissionResult? SubmissionResult { get; init; }
    public string? ErrorMessage { get; init; }
    public bool DidSucceed => SubmissionResult is not null && ErrorMessage is null;
    public List<string> Struggles { get; set; }
    public List<string> LearningOutcomes { get; set; }
    public int EngagementScore { get; set; }
    public Guid ClassroomSessionId { get; set; }
}