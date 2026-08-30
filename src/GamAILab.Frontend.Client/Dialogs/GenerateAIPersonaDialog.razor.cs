using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models.AIPersonaSimulation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class GenerateAIPersonaDialog : ComponentBase
{
    [Parameter] 
    public string AIPersonaDescription { get; set; }
    [Inject] 
    public IAIPersonaSimulationService AIPersonaSimulationService { get; set; } = default!;

    public string _aiPersonaDescription { get; set; }
    private AIPersona? _generatedPersona;
    private bool _isGeneratingPersona;
    
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private async Task OnGenerateAIPersonaClicked()
    {
        try
        {
            _isGeneratingPersona = true;
            
            _generatedPersona = await AIPersonaSimulationService.GenerateAIPersonaAsync(_aiPersonaDescription);
        }
        finally
        {
            _isGeneratingPersona = false;
        }
    }
    
    private async Task OnSaveGeneratedPersonaClicked()
    {
        if (_generatedPersona is not null)
        {
            MudDialog.Close(DialogResult.Ok(_generatedPersona));
        }
    }
    
    private void Cancel() => MudDialog.Cancel();
}