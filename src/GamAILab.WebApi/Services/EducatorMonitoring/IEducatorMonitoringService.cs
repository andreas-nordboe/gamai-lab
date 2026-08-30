using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.Analysis;

namespace GamAILab.WebApi.Services.EducatorMonitoring;

public interface IEducatorMonitoringService
{
    Task PublishLearnerEngagementUpdateAsync(LearnerEngagementLiveUpdate update, CancellationToken cancellationToken = default);
    Task PublishClassroomSimulationStartedAsync(ClassroomSimulation classroomSimulation, CancellationToken cancellationToken = default);
}