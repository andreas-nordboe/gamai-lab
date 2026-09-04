using System.Text.Json;
using GamAILab.Frontend.Client.Dialogs;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class AIPersonaTesting : ComponentBase
{
    [Parameter] public List<AIPersona?> AIPersonas { get; set; } = new();

    public HashSet<AIPersona?> SelectedAIPersonas { get; set; } = new();
    public List<CodeTask?> CodeTasks { get; set; } = new();
    [Inject] public IAIPersonaSimulationService AIPersonaSimulationService { get; set; }
    [Inject] public ICodeTasksService CodeTasksService { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    public CodeTask? SelectedCodeTask { get; set; } 
    private bool _isLoadingAIPersonas;
    private int _executionCounts = 1;
    private int _minutesPerClassroomSimulationStep = 10;
    private int _maxRetriesPerTask = 5;
    private bool _personaSimulationIsRunning;
    private AIPersonaSimulationResponse? _simulationResult;
    private Guid? _classroomSimulationId;
    public IReadOnlyCollection<CodeTask?> SelectedCodeTasks { get; set; } = Array.Empty<CodeTask?>();
    
    [Inject] private IDialogService DialogService { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }
    
    [CascadingParameter(Name = "OnAIPersonaAdded")]
    protected Action<AIPersona>? OnAIPersonaAdded { get; set; }    
    protected override async Task OnInitializedAsync()
    {
        if (OnAIPersonaAdded is not null)
        {
            OnAIPersonaAdded = (newPersona) => 
            {
                AIPersonas.Add(newPersona);
                StateHasChanged();
            };
        }

        // Load Personas
        var aiPersonas = await AIPersonaSimulationService.ListAIPersonasAsync();
        if (aiPersonas.Any())
        {
            AIPersonas = aiPersonas;
        }
        
        // Load Code Tasks
        var codeTasks = await CodeTasksService.ListCodeTasksAsync();
        if (codeTasks.Any())
        {
            CodeTasks = codeTasks;
            // select first one on the UI dropdown
            SelectedCodeTask = codeTasks.FirstOrDefault(); 
            StateHasChanged();
        }
        
    }

    private async Task OnAIPersonaClicked(AIPersona persona)
    {
        if(_personaSimulationIsRunning)
            return;
        
        _personaSimulationIsRunning = true;

        try
        {
            if (SelectedCodeTask is null)
            {
                Snackbar.Add("Failed to run simulation. No selected code task", Severity.Error);
                return;
            }

            var personaList = new List<int>
            {
                persona.Id
            };

            var request = new AIPersonaSimulationRequest
            {
                CodeTaskId = SelectedCodeTask.Id,
                PersonaIds = personaList,
                ExecutionCounts = _executionCounts
            };

            var codeSimulation = await AIPersonaSimulationService.RunAIPersonaCodeEvaluationSimulationAsync(request);
            if (codeSimulation is not null)
            {
                _simulationResult = codeSimulation;
                StateHasChanged();
                
                var parameters = new DialogParameters<CodeTaskFeedbackDialog>
                {
                    { x => x.CodeSubmissionFeedback, codeSimulation.PersonaResults.FirstOrDefault()?.SubmissionResult }
                };
                
                var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Large, CloseButton = true };
                var dialog = await DialogService.ShowAsync<CodeTaskFeedbackDialog>("Code Task Feedback", parameters, options);
                var result = await dialog.Result;
            }
        }
        catch (Exception e)
        {
            Snackbar.Add("Failed to run simulation " + e.Message, Severity.Error);
        }
        finally
        {
            _personaSimulationIsRunning = false;
        }
    }

    private async Task OnDeleteAIPersonaClicked(int aiPersonaId)
    {
        bool? result = await DialogService.ShowMessageBoxAsync(
            "Warning: Delete AI persona?", 
            "Deleting can not be undone!", 
            yesText:"Delete", cancelText:"Cancel");

        if (result is not null)
        {
            var deleteAiPersona = await AIPersonaSimulationService.DeleteAIPersona(aiPersonaId);
            if (deleteAiPersona)
            {
                var aiPersonaToRemove = AIPersonas.Where(aiPersona => aiPersona?.Id == aiPersonaId).FirstOrDefault();
                if (aiPersonaToRemove is not null)
                {
                    AIPersonas.Remove(aiPersonaToRemove);
                    StateHasChanged();
                    Snackbar.Add("Successfully deleted AI persona", Severity.Success);
                }
            }
            else
            {
                Snackbar.Add("Failed to delete AI persona.", Severity.Error);
            }
        }
    }
    
    private async Task EditAIPersonaClicked(AIPersona aiPersona)
    {
        var parameters = new DialogParameters<AddAIPersonaDialog> { { x => x.AIPersona, aiPersona } };
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Medium, CloseButton = true };
        
        var dialog = await DialogService.ShowAsync<AddAIPersonaDialog>($"Edit AI persona with ID: {aiPersona.Id}", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is AIPersona editedAIPersona) 
        {
            var updatedPersona = await AIPersonaSimulationService.AddOrUpdateAIPersona(editedAIPersona);
            if (updatedPersona is not null)
            {
                Snackbar.Add("Successfully AI persona", Severity.Success);
            }
        }
    }

    private async Task OnRunAIPersonaSimulationClicked()
    {
        await RunAIPersonaSimulation(SelectedAIPersonas.ToList());
    }

    private async Task RunAIPersonaSimulation(List<AIPersona?> aiPersonas)
    {
        if(_personaSimulationIsRunning)
            return;
        
        _personaSimulationIsRunning = true;

        try
        {
            if (SelectedCodeTask is null)
            {
                Snackbar.Add("Failed to run simulation. No selected code task", Severity.Error);
                return;
            }

            if (aiPersonas is null || !aiPersonas.Any(p => p is not null))
            {
                Snackbar.Add("Failed to run simulation. No selected AI personas.", Severity.Warning);
                return;
            }

            List<int> personaIds = aiPersonas
                .Where(persona => persona is not null)
                .Select(persona => persona!.Id)
                .ToList();

            var request = new AIPersonaSimulationRequest
            {
                CodeTaskId = SelectedCodeTask.Id,
                PersonaIds = personaIds,
                ExecutionCounts = _executionCounts
            };

            var codeSimulation = await AIPersonaSimulationService.RunAIPersonaCodeEvaluationSimulationAsync(request);
            if (codeSimulation is not null)
            {
                _simulationResult = codeSimulation;
                StateHasChanged();
            }
        }
        catch (Exception e)
        {
            Snackbar.Add("Failed to run simulation " + e.Message, Severity.Error);
        }
        finally
        {
            _personaSimulationIsRunning = false;
        }
    }
    
    private async Task OnAddAIPersonaClicked()
    {
        var newAIPersona = new AIPersona()
        {
            Name = string.Empty,
            Background = string.Empty,
            LearningCapabilities = [],
            LearningDifficulties = [],
            AccessibilityRequirements = [],
            AssignedDifficulty = CodeTaskDifficulty.Beginner,
            // I'll set these fore initialisation purposes, but they will be updated server-side anyways 
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var parameters = new DialogParameters<AddAIPersonaDialog> { { x => x.AIPersona, newAIPersona } };
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Medium, CloseButton = true };
        
        var dialog = await DialogService.ShowAsync<AddAIPersonaDialog>("Add AI Persona", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is AIPersona updatedAIPersona) 
        {
            var updateAiPersona = await AIPersonaSimulationService.AddOrUpdateAIPersona(updatedAIPersona);
            if (updateAiPersona is not null)
            {
                AIPersonas.Add(updateAiPersona);
                Snackbar.Add("Successfully added AI persona", Severity.Success);
            }
        }
    }

    private async Task OnGenerateAIPersonaClicked()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Medium, CloseButton = true };
        
        var dialog = await DialogService.ShowAsync<GenerateAIPersonaDialog>("Generate AI Persona", options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is AIPersona updatedAIPersona) 
        {
            var updateAiPersona = await AIPersonaSimulationService.AddOrUpdateAIPersona(updatedAIPersona);
            if (updateAiPersona is not null)
            {
                AIPersonas.Add(updateAiPersona);
                Snackbar.Add("Successfully generated and saved AI persona", Severity.Success);
            }
        }
        
    }
    
    private async Task OnRunClassroomSimulationClicked()
    {
        if (_personaSimulationIsRunning)
            return;
        
        if (SelectedCodeTasks.Count == 0)
        {
            Snackbar.Add("Failed to run simulation. Select one or more classroom simulation code tasks first.", Severity.Error);
            return;
        }

        try
        {
            var personaIds = SelectedAIPersonas.Where(x => x is not null).Select(x => x!.Id).ToList();
            _classroomSimulationId = Guid.NewGuid();
            _personaSimulationIsRunning = true;
            
            StateHasChanged(); // forces educator dashboard below to update properly!!
            await Task.Yield();

            var request = new ClassroomSimulationRequest
            {
                ClassroomSimulationId = _classroomSimulationId.Value,
                PersonaIds = personaIds,
                // I could potentially add multiple tasks per simulation, but one should work for the report
                CodeTaskIds = SelectedCodeTasks.Where(x =>x is not null).Select(x => x.Id).ToList(),
                MinutesEveryStep = _minutesPerClassroomSimulationStep,
                MaxRetriesPerTask = _maxRetriesPerTask
            };

            await AIPersonaSimulationService.RunClassroomSimulationAsync(request);
        }
        finally
        {
            _personaSimulationIsRunning = false;
        }
    }
}