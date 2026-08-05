namespace GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

public class AIPersonaSimulationResponse
{
    public List<AIPersona> PersonasUsed { get; set; }
    public long ExecutionTimeInMilliseconds { get; set; }
    public DateTime ExecutionTime { get; set; }
}