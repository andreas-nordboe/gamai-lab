
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models.AIPersonaSimulation;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;
using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Components;

public partial class AIPersonaResult : ComponentBase
{
    [Parameter]
    public Guid ClassroomSimulationId { get; set; }

    [Inject]
    public IAnalysisService AnalysisService { get; set; } = default!;

    private ClassroomSimulation? Simulation { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Simulation = await AnalysisService.GetClassroomSimulationByIdAsync(ClassroomSimulationId);
    }

    private async Task SaveRating(
        AIPersonaSimulationResult result)
    {
        //await AnalysisService.SaveResearchEvaluationAsync(ClassroomSimulationId, result);
    }
    
}