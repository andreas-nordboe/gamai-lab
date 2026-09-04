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
    public EngagementDropRiskLevel EngagementDropRiskLevel { get; set; }
    public int PredictedEngagementScore { get; set; }
    public bool PassedLatestCodeTask { get; set; }
    
    // Status updates UI
    public int CurrentTaskNumber { get; set; }
    public int TotalTasks { get; set; }
    public int CurrentStepIndex { get; set; }
}