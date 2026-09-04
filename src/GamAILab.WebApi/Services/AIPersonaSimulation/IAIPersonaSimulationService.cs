using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.WebApi.Services.AIPersonaSimulation;

public interface IAIPersonaSimulationService
{
    Task<AIPersonaSimulationResponse> RunAIPersonaCodeEvaluationSimulationAsync(AIPersonaSimulationRequest request, CancellationToken cancellationToken = default);

    Task<List<AIPersonaSimulationResponse>> RunClassroomSimulationAsync(ClassroomSimulationRequest request, string userId, CancellationToken cancellationToken = default);
    Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync(CancellationToken cancellationToken = default);
    Task<ClassroomSimulation?> GetClassroomSimulationAsync(Guid classroomSimulationId, CancellationToken cancellationToken = default);
    
    // AI persona management (CRUD operations for the frontend)
    Task CreateAIPersona(AIPersona aiPersona, CancellationToken cancellationToken = default);
    public Task<AIPersona?> GetPersonaById(int personaId);
    public Task<List<AIPersona>> GetAllAIPersonas();
    public Task<bool> DeleteAIPersonaById(int aiPersonaId);
    public Task<List<AIPersona>> SeedAIPersonas();
    public Task<AIPersona> AddOrUpdatePersona(AIPersona persona);
    public Task<AIPersona> GenerateAIPersona(string aiPersonaDescription, CancellationToken cancellationToken = default);
}