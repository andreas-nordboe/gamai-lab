using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class AddCodeTaskDialog : ComponentBase
{
    [Parameter] 
    public CodeTask CodeTask { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

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
    
    private void SaveCodeTask() => MudDialog.Close(DialogResult.Ok(CodeTask));
    private void Cancel() => MudDialog.Cancel();
}