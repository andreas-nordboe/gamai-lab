using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GamAILab.WebApi.Hubs;

[Authorize(Roles = "Educator,Admin")]
public class EducatorMonitoringHub : Hub
{
    public async Task JoinClassroomSimulation(Guid classroomSimulationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetClassroomSimulationId(classroomSimulationId));
    }

    public async Task LeaveClassroomSimulation(Guid classroomSimulationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetClassroomSimulationId(classroomSimulationId));
    }
    
    public static string GetClassroomSimulationId(Guid classroomSimulationId)
    {
        return $"classroom-simulation-instance:{classroomSimulationId}";
    }
}