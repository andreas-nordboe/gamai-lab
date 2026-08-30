using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

namespace GamAILab.Shared.Models.Analysis;

public class AIPersonaAnalysisSummaryResponse
{
    public int Id { get; set; }
    public List<AIPersonaSimulationResponse> AIPersonaSimulationResponses { get; set; }
}