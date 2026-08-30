using System.Text.Json;
using GamAILab.Frontend.Client.Components.CodeTasks;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.CodeSubmission;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class CodeTaskFeedbackDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Parameter]
    public CodeSubmissionResult CodeSubmissionFeedback { get; set; }
    
    private void LogOut () => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
   
}