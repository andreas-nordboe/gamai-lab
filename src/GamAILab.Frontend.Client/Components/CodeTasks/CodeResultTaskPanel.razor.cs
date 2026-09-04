using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Components.CodeTasks;

public partial class CodeResultTaskPanel : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; }
    
    private void GoBack()
    {
        NavigationManager.NavigateTo("code-tasks");
    }
}