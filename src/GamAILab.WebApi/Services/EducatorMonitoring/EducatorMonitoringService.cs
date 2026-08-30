using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.Analysis;
using GamAILab.WebApi.Data;
using GamAILab.WebApi.Hubs;
using GamAILab.WebApi.Services.LLMService;
using Microsoft.AspNetCore.SignalR;

namespace GamAILab.WebApi.Services.EducatorMonitoring;

public class EducatorMonitoringService : IEducatorMonitoringService
{
    private readonly ILogger<EducatorMonitoringService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<EducatorMonitoringHub> _hubContext;

    public EducatorMonitoringService(ILogger<EducatorMonitoringService> logger, ApplicationDbContext dbContext, IHubContext<EducatorMonitoringHub> hubContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _hubContext = hubContext;
    }
    
    public async Task PublishLearnerEngagementUpdateAsync(LearnerEngagementLiveUpdate update, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(EducatorMonitoringHub.GetClassroomSimulationId(update.ClassroomSimulationId))
            .SendAsync("LearnerEngagementUpdated", update, cancellationToken);
    }

    public async Task PublishClassroomSimulationStartedAsync(ClassroomSimulation classroomSimulation, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("ClassroomSimulationStarted", classroomSimulation, cancellationToken);
    }


    private static bool DetectEngagementDecline(IReadOnlyList<int> scores)
    {
        if (scores.Count < 3)
            return false;

        // checks if the scores decline based on the last 3 scores
        return scores[^3] > scores[^2] && scores[^2] > scores[^1];
    }
}