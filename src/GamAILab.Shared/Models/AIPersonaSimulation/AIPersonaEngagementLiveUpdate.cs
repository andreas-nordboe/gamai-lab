namespace GamAILab.Shared.Models.AIPersonaSimulation;

// For SignalR live updates
public class LearnerEngagementLiveUpdate
{
    public Guid ClassroomSimulationId { get; set; }
    public int PersonaId { get; set; }
    public string PersonaName { get; set; } = string.Empty;
    public int EngagementScore { get; set; }
    public int SimulatedMinute { get; set; }
    public List<string> Struggles { get; set; } = [];
    public List<string> LearningOutcomes { get; set; } = [];
    public bool EngagementIsDeclining { get; set; } // for showing a warning on the frontend
}