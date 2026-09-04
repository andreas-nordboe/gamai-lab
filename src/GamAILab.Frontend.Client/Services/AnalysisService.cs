using System.Net;
using System.Net.Http.Json;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.Frontend.Client.Services;

public class AnalysisService : IAnalysisService
{
    private readonly HttpClient _httpClient;

    public AnalysisService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AIPersonaSimulationResponse>> GetAIPersonaAnalysisSummaryAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<List<AIPersonaSimulationResponse>>("api/analysis/summary", cancellationToken)?? [];
    }

    public async Task<AIPersonaSimulationResponse?> GetAIPersonaAnalysisSummaryByIdAsync(int summaryId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/analysis/{summaryId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AIPersonaSimulationResponse>(cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAIPersonaAnalysisSummaryAsync(int summaryId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/analysis/delete/{summaryId}", cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ClassroomSimulation>>("api/ai-personas/classroom-simulations") ?? [];
    }

    public async Task<ClassroomSimulation?> GetClassroomSimulationByIdAsync(Guid classroomSimulationId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ai-personas/classroom-simulations/{classroomSimulationId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ClassroomSimulation>(cancellationToken: cancellationToken);
    }
}