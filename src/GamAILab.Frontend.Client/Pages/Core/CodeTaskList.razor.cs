using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTaskList : ComponentBase
{
    private List<CodeTask?> _codeTasks = new List<CodeTask?>();
    [Inject] public ICodeTasksService CodeTasksService { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    private bool _isLoadingCodeTasks;
    
    protected override async Task OnInitializedAsync()
    {
        var codeTasks = await CodeTasksService.ListCodeTasksAsync();
        if (codeTasks.Any())
        {
            _codeTasks = codeTasks;
        }
    }

    private void OnTaskClicked(CodeTask task)
    {
        NavigationManager.NavigateTo($"code-task/{task.Id}");
    }
}