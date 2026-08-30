namespace GamAILab.Shared.Models.AIPersonaSimulation;

// fake "memory/state" for the persona during the simulation so they remember how they did on their previous attempt 
public class AIPersonaMemoryState
{
    public int PreviousEngagementScore { get; set; }

    public List<string> PreviousStruggles { get; set; } = [];

    public List<string> PreviousLearningOutcomes { get; set; } = [];

    public string? PreviousTaskOutcome { get; set; }
}