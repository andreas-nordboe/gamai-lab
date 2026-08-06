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
    private bool _personaSimulationIsRunning;

    private string _simulationResults; // TODO for temporary debugging during development only: replace with proper dialog summary later
    
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
        }
        
    }

    private async Task OnAIPersonaClicked(AIPersona persona)
    {
        // Clear and then set this specific persona for now
        SelectedAIPersonas = null;
        
        await RunAIPersonaSimulation(new List<AIPersona?>
        {
            persona
        });
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
                // TODO display output temporarily (presenting for supervisor tomorrow) 
                _simulationResults = JsonSerializer.Serialize(codeSimulation);
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
}