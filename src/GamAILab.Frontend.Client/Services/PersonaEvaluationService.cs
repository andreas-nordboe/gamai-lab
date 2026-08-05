using System.Net;
using System.Net.Http.Json;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

namespace GamAILab.Frontend.Client.Services;

public class PersonaEvaluationService : IPersonaEvaluationService
{
    private readonly HttpClient _httpClient;

    public PersonaEvaluationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AIPersonaSimulationResponse?> RunAIPersonaCodeEvaluationSimulationAsync(AIPersonaSimulationRequest request, CancellationToken cancellationToken = default)
    {
        var addOrUpdateAiPersona = await _httpClient.PostAsJsonAsync($"api/ai-personas/run-simulation", request);
        if (!addOrUpdateAiPersona.IsSuccessStatusCode)
            return null;
        
        return await addOrUpdateAiPersona.Content.ReadFromJsonAsync<AIPersonaSimulationResponse>();
    }

    public async Task<List<AIPersona>> ListAIPersonasAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AIPersona>>("api/ai-personas/ai-personas") ?? [];
    }

    public async Task<AIPersona?> GetAIPersonaAsync(int aiPersonaId)
    {
        var aiPersona = await _httpClient.GetAsync($"api/ai-personas/{aiPersonaId}");
        if (aiPersona.StatusCode == HttpStatusCode.NotFound || !aiPersona.IsSuccessStatusCode)
            return null;
        
        return await aiPersona.Content.ReadFromJsonAsync<AIPersona>();
    }

    public async Task<bool> DeleteAIPersona(int aiPersonaId)
    {
        var aiPersona = await _httpClient.DeleteAsync($"api/ai-personas/delete/{aiPersonaId}");
        if (aiPersona.StatusCode == HttpStatusCode.NotFound || !aiPersona.IsSuccessStatusCode)
            return false;
        
        return await aiPersona.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<AIPersona?> AddOrUpdateAIPersona(AIPersona aiPersona)
    {
        var addOrUpdateAiPersona = await _httpClient.PostAsJsonAsync($"api/code-tasks/add-or-update", aiPersona);
        if (!addOrUpdateAiPersona.IsSuccessStatusCode)
            return null;
        
        return await addOrUpdateAiPersona.Content.ReadFromJsonAsync<AIPersona>();
    }
}