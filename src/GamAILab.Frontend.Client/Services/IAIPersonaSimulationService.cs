using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;

namespace GamAILab.Frontend.Client.Services;

public interface IAIPersonaSimulationService
{
    Task<AIPersonaSimulationResponse?> RunAIPersonaCodeEvaluationSimulationAsync(AIPersonaSimulationRequest request, CancellationToken cancellationToken = default);
    Task <List<AIPersona?>> ListAIPersonasAsync();
    Task<AIPersona> GetAIPersonaAsync(int aiPersonaId);
    Task<bool> DeleteAIPersona(int aiPersonaId);
    Task<AIPersona?> AddOrUpdateAIPersona(AIPersona aiPersona); 
}