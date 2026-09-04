using System.ComponentModel.DataAnnotations.Schema;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

namespace GamAILab.Shared.Models.Analysis;

public class ClassroomSimulation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ClassroomSimulationStatus Status { get; set; } = ClassroomSimulationStatus.Running;
    public string InitiatedByUserId { get; set; } = string.Empty;
    public List<AIPersonaSimulationResponse> SimulationResponses { get; set; } = [];
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int LearnerCount { get; set; }
    [NotMapped]
    public List<LearnerEngagementLiveUpdate> LearnerUpdates => SimulationResponses.SelectMany(simulation =>
    simulation.PersonaResults.Select(result =>
        new LearnerEngagementLiveUpdate
        {
            ClassroomSimulationId = Id,
            PersonaId = result.Persona.Id,
            PersonaName = result.Persona.Name,
            EngagementScore = result.EngagementScore,
            SimulatedMinute = simulation.SimulatedMinute,
            Struggles = result.Struggles,
            LearningOutcomes = result.LearningOutcomes
        }))
    .ToList();
}