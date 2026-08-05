using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class RunAIPersonaSimulationDialog : ComponentBase
{
    [Parameter] 
    public AIPersonaSimulationRequest AiPersonaSimulationRequest { get; set; }
    [Inject] 
    public ICodeTasksService CodeTasksService { get; set; }

    public List<CodeTask?> CodeTasks { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        CodeTasks = await CodeTasksService.ListCodeTasksAsync();
    }

    private Task RunCodeSimulation()
    {
        throw new NotImplementedException();
    }
    
    private Task StopCodeSimulation()
    {
        throw new NotImplementedException();
    }
}