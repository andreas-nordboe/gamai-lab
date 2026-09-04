using System.Net;
using System.Net.Http.Json;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.Frontend.Client.Services;

public class AIPersonaSimulationService : IAIPersonaSimulationService
{
    private readonly HttpClient _httpClient;

    public AIPersonaSimulationService(HttpClient httpClient)
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
    
    public async Task<List<AIPersonaSimulationResponse>?> RunClassroomSimulationAsync(ClassroomSimulationRequest request, CancellationToken cancellationToken = default)
    {
        var addOrUpdateAiPersona = await _httpClient.PostAsJsonAsync($"api/ai-personas/run-classroom-simulation", request);
        if (!addOrUpdateAiPersona.IsSuccessStatusCode)
            return null;
        
        return await addOrUpdateAiPersona.Content.ReadFromJsonAsync<List<AIPersonaSimulationResponse>?>();
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
        var addOrUpdateAiPersona = await _httpClient.PostAsJsonAsync($"api/ai-personas/add-or-update", aiPersona);
        if (!addOrUpdateAiPersona.IsSuccessStatusCode)
            return null;
        
        return await addOrUpdateAiPersona.Content.ReadFromJsonAsync<AIPersona>();
    }

    public async Task<AIPersona?> GenerateAIPersonaAsync(string aiPersonaDescription)
    {
        var addOrUpdateAiPersona = await _httpClient.PostAsJsonAsync($"api/ai-personas/generate", new GenerateAIPersonaRequest
        {
            Description = aiPersonaDescription
        });
        if (!addOrUpdateAiPersona.IsSuccessStatusCode)
            return null;
        
        return await addOrUpdateAiPersona.Content.ReadFromJsonAsync<AIPersona>();
    }
    
    public async Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ClassroomSimulation>>("api/ai-personas/classroom-simulations") ?? [];
    }
    
    public async Task<ClassroomSimulation?> GetClassroomSimulationByIdAsync(Guid classroomId, CancellationToken cancellationToken = default)
    {
        var classroomSimulation = await _httpClient.GetAsync($"api/ai-personas/classroom-simulations/{classroomId}");
        
        if (classroomSimulation.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!classroomSimulation.IsSuccessStatusCode)
            return null;

        return await classroomSimulation.Content.ReadFromJsonAsync<ClassroomSimulation>(cancellationToken);
    }
}