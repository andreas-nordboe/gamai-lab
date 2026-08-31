
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.Analysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class EducatorMonitoring : ComponentBase, IDisposable
{
    [Inject]
    NavigationManager NavigationManager { get; set; }  
    [Inject] 
    EducatorMonitoringService EducatorMonitoringService { get; set; }
    [Inject] 
    IAIPersonaSimulationService AIPersonaSimulationService { get; set; }
    
    [Parameter]
    public Guid? _classroomSimulationId { get; set; }
    private bool _isConnected;
    private readonly List<LearnerEngagementLiveUpdate> _learners = [];
    private List<ClassroomSimulation> ClassroomSimulations = [];
    private ClassroomSimulation? SelectedClassroomSimulation;
    private readonly List<LearnerEngagementLiveUpdate> _learnerEngagements = [];
    private int _totalLearnersDeclining => _learners.Count(x => x.EngagementIsDeclining);
    private double _averageEngagement => _learners.Count == 0 ? 0 : _learners.Average(x => x.EngagementScore);
    private int _highRiskCount => _learners.Count(x => x.EngagementDropRiskLevel == EngagementDropRiskLevel.High);
    private Guid? _joinedClassroomId;
    private int _passedLatestTaskCount => _learners.Count(x => x.PassedLatestCodeTask);
    
    // Chart
    private string[] _timeLabels = [];
    private List<ChartSeries<double>> _engagementHistory = [];
    private readonly LineChartOptions _chartOptions = new()
    {
        XAxisTitle = "Simulated Time",
        YAxisTitle = "Average Engagement",
        ShowDataMarkers = true
    };
    
    // Status
    
    protected override async Task OnInitializedAsync()
    {
        // SignalR bindings
        EducatorMonitoringService.EngagementUpdated += OnEngagementUpdated;
        EducatorMonitoringService.ClassroomSimulationStarted += OnClassroomSimulationStarted;
        EducatorMonitoringService.ClassroomSimulationCompleted += OnClassroomSimulationCompleted;
        await EducatorMonitoringService.StartAsync("http://localhost:5270/hubs/educator-monitoring");
        _isConnected = true;
        
        ClassroomSimulations = await AIPersonaSimulationService.ListClassroomSimulationsAsync();

        if (SelectedClassroomSimulation is null && ClassroomSimulations.Count > 0)
        {
            // newest running classroom simulation
            var simulation = ClassroomSimulations.Where(x => x.Status == ClassroomSimulationStatus.Running)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefault() ?? ClassroomSimulations
            .OrderByDescending(x => x.StartedAt)
            .First();
            
            if (simulation is not null)
            {
                await SelectClassroomSimulationAsync(simulation);
            }
        }
    }
    
    // This seems to work better by reacting to parameter change AFTER connecting to the hub
    protected override async Task OnParametersSetAsync()
    {
        if (!_classroomSimulationId.HasValue)
            return;

        var simulation = ClassroomSimulations.Where(x => x.Status == ClassroomSimulationStatus.Running).OrderByDescending(x => x.StartedAt).FirstOrDefault();

        if (simulation is not null)
        {
            await SelectClassroomSimulationAsync(simulation);
        }
    }

    private static Color GetEngagementColor(int engagement)
    {
        return engagement switch
        {
            < 40 => Color.Error,
            < 65 => Color.Warning,
            _ => Color.Success
        };
    }

    private static Color GetEngagementRiskColor(EngagementDropRiskLevel engagementDropRiskLevel)
    {
        return engagementDropRiskLevel switch
        {
            EngagementDropRiskLevel.Low => Color.Success,
            EngagementDropRiskLevel.Medium => Color.Warning,
            EngagementDropRiskLevel.High => Color.Error,
            _ => Color.Default
        };
    }
    
    private void OnEngagementUpdated(LearnerEngagementLiveUpdate update)
    {
        _learnerEngagements.Add(update);
        
        var learner = _learners.FirstOrDefault(x => x.PersonaId == update.PersonaId);

        if (learner is null)
        {
            _learners.Add(update);
        }
        else
        {
            learner.EngagementScore = update.EngagementScore;
            learner.EngagementIsDeclining = update.EngagementIsDeclining;
            learner.SimulatedMinute = update.SimulatedMinute;
            learner.Struggles = update.Struggles;
            learner.LearningOutcomes = update.LearningOutcomes;
            learner.PredictedEngagementScore = update.PredictedEngagementScore;
            learner.EngagementDropRiskLevel = update.EngagementDropRiskLevel;
            learner.PassedLatestCodeTask = update.PassedLatestCodeTask;
            learner.CurrentTaskNumber = update.CurrentTaskNumber;
            learner.TotalTasks = update.TotalTasks;
            learner.CurrentStepIndex = update.CurrentStepIndex;
        }

        UpdateEngagementTimeline();

        InvokeAsync(StateHasChanged);
    }

    private async void OnClassroomSimulationStarted(ClassroomSimulation classroomSimulation)
    {
        // UI handler Blazor trick instead of void
        _ = InvokeAsync(async () =>
        {
            var existing = ClassroomSimulations.FirstOrDefault(x => x.Id == classroomSimulation.Id);

            if (existing is null)
            {
                ClassroomSimulations.Insert(0, classroomSimulation);
            }

            if (_classroomSimulationId == classroomSimulation.Id)
            {
                await SelectClassroomSimulationAsync(classroomSimulation);
            }

            StateHasChanged();
        });
    }
    
    private async void OnClassroomSimulationCompleted(ClassroomSimulation classroomSimulation)
    {
        var existing = ClassroomSimulations.FirstOrDefault(x => x.Id == classroomSimulation.Id);

        if (existing is not null)
        {
            existing.Status = classroomSimulation.Status;
            existing.CompletedAt = classroomSimulation.CompletedAt;
        }

        if (SelectedClassroomSimulation?.Id == classroomSimulation.Id)
        {
            SelectedClassroomSimulation.Status = classroomSimulation.Status;
            SelectedClassroomSimulation.CompletedAt = classroomSimulation.CompletedAt;
        }

        StateHasChanged();
    }

    public void Dispose()
    {
        EducatorMonitoringService.EngagementUpdated -= OnEngagementUpdated;
        EducatorMonitoringService.ClassroomSimulationStarted -= OnClassroomSimulationStarted;
        EducatorMonitoringService.ClassroomSimulationCompleted -= OnClassroomSimulationCompleted;
    }
    
    private static Color GetSimulationStatusColor(ClassroomSimulationStatus status)
    {
        return status switch
        {
            ClassroomSimulationStatus.Running => Color.Info,
            ClassroomSimulationStatus.Completed => Color.Success,
            ClassroomSimulationStatus.Failed => Color.Error,
            // TODO I could possibly add cancelled status as well (yellow warning)
            _ => Color.Default
        };
    }
    
    private async Task SelectClassroomSimulationAsync(ClassroomSimulation classroomSimulation)
    {
        if (_joinedClassroomId.HasValue)
        {
            await EducatorMonitoringService.LeaveClassroomAsync(_joinedClassroomId.Value);
            _joinedClassroomId = null;
        }

        SelectedClassroomSimulation = classroomSimulation;
        
        // Clearing fixes previous UI issues
        _learners.Clear();
        _learnerEngagements.Clear();
        _engagementHistory = [];
        _timeLabels = [];
        
        await LoadClassroomSimulationAsync(classroomSimulation.Id);

        if (classroomSimulation.Status == ClassroomSimulationStatus.Running)
        {
            await EducatorMonitoringService
                .JoinClassroomAsync(classroomSimulation.Id);

            _joinedClassroomId = classroomSimulation.Id;
        }
        
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task LoadClassroomSimulationAsync(Guid classroomSimulationId)
    {
        var simulation = await AIPersonaSimulationService.GetClassroomSimulationByIdAsync(classroomSimulationId);

        if (simulation is null)
        {
            return;
        }

        _learners.Clear();
        _learnerEngagements.Clear();

        if (simulation.LearnerUpdates is not null)
        {
            _learnerEngagements.AddRange(simulation.LearnerUpdates);
            
            // Get the latest state for all of them
            var latestLearners = simulation.LearnerUpdates
                .GroupBy(x => x.PersonaId)
                .Select(group => group
                    .OrderByDescending(x => x.SimulatedMinute)
                    .First())
                .ToList();

            // adds bulk inserts at the end
            _learners.AddRange(latestLearners);

            UpdateEngagementTimeline();
        }
    }
    
    private string GetSimulationButtonTitle(ClassroomSimulation simulation)
    {
        var selected = SelectedClassroomSimulation?.Id == simulation.Id;

        if (simulation.Status == ClassroomSimulationStatus.Running)
        {
            return selected ? "Monitoring" : "Monitor";
        }
        else
        {
            return selected ? "Reviewing" : "Review";
        }
    }

    private void UpdateEngagementTimeline()
    {
        var engagementHistory = _learnerEngagements
        .GroupBy(x => x.SimulatedMinute)
        .OrderBy(x => x.Key)
        .Select(x => new
        { Minute = x.Key, AverageEngagement = x.Average(y => y.EngagementScore) })
        .ToList();

        _engagementHistory =
        [
            new ChartSeries<double>
            {
                Name = "Average Engagement",
                Data = engagementHistory
                    .Select(x => x.AverageEngagement)
                    .ToArray()
            }
        ];
        _timeLabels = engagementHistory.Select(x => $"{x.Minute} min").ToArray();
    }
}