using System.Text.Json;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class AddCodeTaskDialog : ComponentBase
{
    [Parameter] 
    public CodeTask CodeTask { get; set; }
    [Parameter] 
    public bool IsEditing { get; set; }
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Inject] public ICodeTasksService CodeTasksService { get; set; }

    private async Task OnAddExampleClicked()
    {
        CodeTask.Examples.Add(string.Empty);
    }

    private async Task OnAddConstraintClicked()
    {
        CodeTask.Constraints.Add(string.Empty);
    }
    
    private void RemoveExample(int index)
    {
        if (index >= 0 && index < CodeTask.Examples.Count)
        {
            CodeTask.Examples.RemoveAt(index);
        }
    }

    private void RemoveConstraint(int index)
    {
        if (index >= 0 && index < CodeTask.Constraints.Count)
        {
            CodeTask.Constraints.RemoveAt(index);
        }
    }

    private async Task GenerateCodeEvaluationPlanClicked()
    {
        var updatedCodeTask = await CodeTasksService.ReGenerateCodeEvaluationPlanAsync(CodeTask.Id);
        if (updatedCodeTask is not null)
        {
            CodeTask = updatedCodeTask;
            StateHasChanged();
        }
    }

    private string DisplayCodeEvaluationPlan(CodeTask codeTask)
    {
        return JsonSerializer.Serialize(codeTask.AiCodeEvaluationPlan);
    }

    private async Task OnRegenerateCodePlanClicked()
    {
        await CodeTasksService.ReGenerateCodeEvaluationPlanAsync(CodeTask.Id);
        
    }
    
    private void SaveCodeTask() => MudDialog.Close(DialogResult.Ok(CodeTask));
    private void Cancel() => MudDialog.Cancel();
}