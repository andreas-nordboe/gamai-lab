using System.Text.Json;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class AddCodeTaskDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Parameter] 
    public CodeTask CodeTask { get; set; }
    [Parameter] 
    public bool IsEditing { get; set; }
    
    private void SaveCodeTask() => MudDialog.Close(DialogResult.Ok(CodeTask));
    private void Cancel() => MudDialog.Cancel();
}