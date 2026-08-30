namespace GamAILab.Shared.Models.AIPersonaSimulation;

public class ClassroomSimulationRequest
{
    public Guid? ClassroomSimulationId { get; set; }
    public List<int> PersonaIds { get; set; } = [];
    public List<int> CodeTaskIds { get; set; } = [];
    public int MinutesEveryStep { get; set; } = 5;
    public int MaxRetriesPerTask { get; set; } = 1;
}