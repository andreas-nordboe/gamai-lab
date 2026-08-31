using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GamAILab.WebApi.Hubs;

[Authorize(Roles = "Learner,Educator,Admin,Researcher")]
public class GameHub : Hub
{
    
}