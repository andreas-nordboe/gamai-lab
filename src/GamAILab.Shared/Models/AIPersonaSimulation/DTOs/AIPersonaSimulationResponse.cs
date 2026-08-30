using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

public class AIPersonaSimulationResponse
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public List<AIPersona> PersonasUsed { get; set; } = [];
    public long ExecutionTimeInMilliseconds { get; set; }
    public Guid SimulationId { get; init; } // TODO could potentially be string for parsing issues
    public int CodeTaskId { get; init; }
    public string CodeTaskTitle { get; set; } // for UI instead of sending back the entire task, although the task can be retrieved from the frontend dashboard (extra API call)
    public string LlmModelUsed { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime CompletedAt { get; init; }
    public List<AIPersonaSimulationResult> PersonaResults { get; init; } = [];
    
    // Model calculation instead of doing this in the service
    public int AIPersonaTotalCount => PersonaResults.Count;
    public int SuccessfulPersonasCount => PersonaResults.Count(result => result.DidSucceed);
    public int FailedPersonasCount => PersonaResults.Count(result => !result.DidSucceed);
    public long DurationInMilliseconds => Math.Max(0, (long)(CompletedAt - StartedAt).TotalMilliseconds);
    // for classroom simulations tables (results section for the report)
    public Guid? ClassroomSimulationId { get; set; }
    public int SimulationTimeStepIndex { get; set; }
    public int SimulatedMinute { get; set; }
    public int AttemptNumber { get; set; } = 1;
}