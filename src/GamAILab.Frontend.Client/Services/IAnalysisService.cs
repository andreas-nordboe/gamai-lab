using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

namespace GamAILab.Frontend.Client.Services;

public interface IAnalysisService
{
    public Task<List<AIPersonaSimulationResponse>> GetAIPersonaAnalysisSummaryAsync(CancellationToken cancellationToken = default);

    public Task<AIPersonaSimulationResponse?> GetAIPersonaAnalysisSummaryByIdAsync(int summaryId, CancellationToken cancellationToken = default);

    public Task<bool> DeleteAIPersonaAnalysisSummaryAsync(int summaryId, CancellationToken cancellationToken = default);
}