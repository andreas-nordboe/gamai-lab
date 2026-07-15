using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Dialogs;

public partial class LogoutDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private void Submit() => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
}