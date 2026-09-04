using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.Frontend.Client.Services;

public interface IAnalysisService
{
    public Task<List<AIPersonaSimulationResponse>> GetAIPersonaAnalysisSummaryAsync(CancellationToken cancellationToken = default);
    public Task<AIPersonaSimulationResponse?> GetAIPersonaAnalysisSummaryByIdAsync(int summaryId, CancellationToken cancellationToken = default);
    public Task<bool> DeleteAIPersonaAnalysisSummaryAsync(int summaryId, CancellationToken cancellationToken = default);
    public Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync();
    public Task<ClassroomSimulation?> GetClassroomSimulationByIdAsync(Guid classroomSimulationId, CancellationToken cancellationToken = default);
}