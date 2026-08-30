using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.Frontend.Client.Services;

public interface IAIPersonaSimulationService
{
    Task<AIPersonaSimulationResponse?> RunAIPersonaCodeEvaluationSimulationAsync(AIPersonaSimulationRequest request, CancellationToken cancellationToken = default);
    Task<List<AIPersonaSimulationResponse>?> RunClassroomSimulationAsync(ClassroomSimulationRequest request, CancellationToken cancellationToken = default);
    Task <List<AIPersona?>> ListAIPersonasAsync();
    Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync();
    Task<ClassroomSimulation?> GetClassroomSimulationByIdAsync(Guid classroomId, CancellationToken cancellationToken = default);
    Task<AIPersona> GetAIPersonaAsync(int aiPersonaId);
    Task<bool> DeleteAIPersona(int aiPersonaId);
    Task<AIPersona?> AddOrUpdateAIPersona(AIPersona aiPersona); 
    Task<AIPersona?> GenerateAIPersonaAsync(string aiPersonaDescription); 
}