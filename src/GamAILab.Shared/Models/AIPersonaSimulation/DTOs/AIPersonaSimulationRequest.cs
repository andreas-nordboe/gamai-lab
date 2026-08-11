namespace GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

public class AIPersonaSimulationRequest
{
    public int CodeTaskId { get; set; }
    public List<int> PersonaIds { get; set; }
    public int ExecutionCounts { get; set; } // How many times it will execute, perhaps wait until every person has finished first?
}