using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.Shared.Models.AIPersonaSimulation;

public class AIPersonaSimulationResult
{
    public AIPersona Persona { get; set; }
    public string? CodeAttempt { get; init; }
    public CodeSubmissionResult? SubmissionResult { get; init; }
    public string? ErrorMessage { get; init; }
    public bool DidSucceed => SubmissionResult is not null && ErrorMessage is null;
}