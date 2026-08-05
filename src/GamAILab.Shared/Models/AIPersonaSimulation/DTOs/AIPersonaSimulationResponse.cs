namespace GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

public class AIPersonaSimulationResponse
{
    public List<AIPersona> PersonasUsed { get; set; }
    public long ExecutionTimeInMilliseconds { get; set; }
    public DateTime ExecutionTime { get; set; }
    
    public Guid SimulationId { get; init; }
    public int CodeTaskId { get; init; }
    public string LlmModelUsed { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime CompletedAt { get; init; }
    public List<AIPersonaSimulationResult> PersonaResults { get; init; } = [];
    public int AIPersonaTotalCount => PersonaResults.Count;
    public int SuccessfulPersonasCount => PersonaResults.Count(result => result.DidSucceed);
    public int FailedPersonasCount => PersonaResults.Count(result => !result.DidSucceed);
    public long DurationInMilliseconds => Math.Max(0, (long)(CompletedAt - StartedAt).TotalMilliseconds);
}