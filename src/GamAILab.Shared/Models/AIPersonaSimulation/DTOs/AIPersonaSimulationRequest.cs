using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

public class AIPersonaSimulationRequest
{
    public int CodeTaskId { get; set; }
    public List<int> PersonaIds { get; set; }
    public int ExecutionCounts { get; set; } // How many times it will execute, perhaps wait until every person has finished first?
    public Guid? ClassroomSimulationId { get; set; }
    public int SimulationTimeStepIndex { get; set; }
    public int SimulatedMinute { get; set; }
    public int AttemptNumber { get; set; } = 1;
    [JsonIgnore]
    public Dictionary<int, AIPersonaMemoryState>? PersonaStates { get; set; }
}