using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.WebApi.Services.Analysis;

public interface IAnalysisService
{
    Task<List<AIPersonaSimulationResponse>> GetAIPersonaAnalysisSummaryAsync(CancellationToken cancellationToken = default);
    public Task<AIPersonaSimulationResponse?> GetAIPersonaAnalysisSummaryByIdAsync(int summaryId, CancellationToken cancellationToken = default);
    public Task<bool> DeleteAIPersonaAnalysisSummaryAsync(int summaryId, CancellationToken cancellationToken = default);
}