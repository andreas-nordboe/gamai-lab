using Blazored.LocalStorage;
using GamAILab.Frontend.Client.Providers;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.Analysis;
using Microsoft.AspNetCore.SignalR.Client;

namespace GamAILab.Frontend.Client.Services;

public class EducatorMonitoringService : IAsyncDisposable // not includign this caused issues before
{
    private HubConnection? _webSocketConnection;
    private readonly ILocalStorageService _localStorage;

    public EducatorMonitoringService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public event Action<LearnerEngagementLiveUpdate>? EngagementUpdated;
    public event Action<ClassroomSimulation>? ClassroomSimulationStarted;
    public bool IsConnected => _webSocketConnection?.State == HubConnectionState.Connected;
    
    public async Task StartAsync(string hubUrl)
    {
        if (_webSocketConnection?.State == HubConnectionState.Connected)
            return;
        
        var accessToken = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        if (_webSocketConnection is null)
        {
            _webSocketConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();
            
            _webSocketConnection.On<LearnerEngagementLiveUpdate>("LearnerEngagementUpdated",
            update =>
            {
                EngagementUpdated?.Invoke(update);
            });
            
            _webSocketConnection.On<ClassroomSimulation>("ClassroomSimulationStarted", async simulation =>
            {
                ClassroomSimulationStarted?.Invoke(simulation);
            });
        }
        
        await _webSocketConnection.StartAsync();
    }

    public async Task JoinClassroomAsync(Guid classroomSimulationId)
    {
        if (_webSocketConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("SignalR is not connected!!");
        }

        await _webSocketConnection.SendAsync("JoinClassroomSimulation", classroomSimulationId);
    }
    
    public async Task LeaveClassroomAsync(Guid classroomSimulationId)
    {
        if (_webSocketConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("SignalR is not connected!!");
        }

        await _webSocketConnection.SendAsync("LeaveClassroomSimulation", classroomSimulationId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_webSocketConnection is not null)
        {
                await _webSocketConnection.DisposeAsync();
        }
    }
}