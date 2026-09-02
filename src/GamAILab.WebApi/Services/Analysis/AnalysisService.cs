using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Services.CodeTasks;
using Microsoft.EntityFrameworkCore;

namespace GamAILab.WebApi.Services.Analysis;

public class AnalysisService : IAnalysisService
{
    private readonly ILogger<AnalysisService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AnalysisService(ApplicationDbContext dbContext, ILogger<AnalysisService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<List<AIPersonaSimulationResponse>> GetAIPersonaAnalysisSummaryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AIPersonaSimulations.ToListAsync();
    }

    public async Task<AIPersonaSimulationResponse?> GetAIPersonaAnalysisSummaryByIdAsync(int summaryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AIPersonaSimulations
            .FirstOrDefaultAsync(x => x.Id == summaryId, cancellationToken);
    }

    public async Task<bool> DeleteAIPersonaAnalysisSummaryAsync(int summaryId, CancellationToken cancellationToken = default)
    {
        var summary = await _dbContext.AIPersonaSimulations
            .FirstOrDefaultAsync(x => x.Id == summaryId, cancellationToken);

        if (summary is null)
            return false;

        _dbContext.AIPersonaSimulations.Remove(summary);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<ClassroomSimulation>> ListClassroomSimulationsAsync(CancellationToken cancellationToken = default)
    {
        // load everythign basically 
        return await _dbContext.ClassroomSimulations
            .AsNoTracking()
            .Include(x => x.SimulationResponses)
            .ThenInclude(x => x.PersonaResults)
            .ThenInclude(x => x.Persona)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<ClassroomSimulation?> GetClassroomSimulationByIdAsync(Guid classroomSimulationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClassroomSimulations
            .AsNoTracking()
            .Include(x => x.SimulationResponses)
            .ThenInclude(x => x.PersonaResults)
            .ThenInclude(x => x.Persona)
            .FirstOrDefaultAsync(
                x => x.Id == classroomSimulationId,
                cancellationToken);
    }
    
}