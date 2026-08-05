using GamAILab.Shared.Models.AIPersonaSimulation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class AddAIPersonaDialog : ComponentBase
{
    [Parameter] 
    public AIPersona AIPersona { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private async Task AddLearningCapabilitiesClicked()
    {
        AIPersona.LearningCapabilities.Add(string.Empty);
    }

    private async Task AddLearningDifficultiesClicked()
    {
        AIPersona.LearningDifficulties.Add(string.Empty);
    }
    
    private async Task AddAccessibilityRequirementsClicked()
    {
        AIPersona.AccessibilityRequirements.Add(string.Empty);
    }
    
    
    private void RemoveLearningCapability(int index)
    {
        if (index >= 0 && index < AIPersona.LearningCapabilities.Count)
        {
            AIPersona.LearningCapabilities.RemoveAt(index);
        }
    }

    private void RemoveLearningDifficulty(int index)
    {
        if (index >= 0 && index < AIPersona.LearningDifficulties.Count)
        {
            AIPersona.LearningDifficulties.RemoveAt(index);
        }
    }

    private void RemoveAccessibilityRequirement(int index)
    {
        if (index >= 0 && index < AIPersona.AccessibilityRequirements.Count)
        {
            AIPersona.AccessibilityRequirements.RemoveAt(index);
        }
    }
    
    private void SaveAIPersona() => MudDialog.Close(DialogResult.Ok(AIPersona));
    private void Cancel() => MudDialog.Cancel();
}