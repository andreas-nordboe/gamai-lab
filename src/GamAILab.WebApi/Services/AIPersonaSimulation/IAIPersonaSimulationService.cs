using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

namespace GamAILab.WebApi.Services.AIPersonaSimulation;

public interface IAIPersonaSimulationService
{
    Task<AIPersonaSimulationResponse> RunAIPersonaCodeEvaluationSimulationAsync(AIPersonaSimulationRequest request, CancellationToken cancellationToken = default);
    
    // AI persona management (CRUD operations for the frontend)
    Task CreateAIPersona(AIPersona aiPersona, CancellationToken cancellationToken = default);
    public Task<AIPersona?> GetPersonaById(int personaId);
    public Task<List<AIPersona>> GetAllAIPersonas();
    public Task<bool> DeleteAIPersonaById(int aiPersonaId);
    public Task<List<AIPersona>> SeedAIPersonas();
    public Task<AIPersona> AddOrUpdatePersona(AIPersona persona);
}