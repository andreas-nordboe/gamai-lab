using GamAILab.Frontend.Client.Dialogs;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTaskManagement : ComponentBase
{
    [Inject] 
    public ICodeTasksService CodeTasksService { get; set; }
    [Inject]
    public IDialogService DialogService { get; set; }
    [Inject]
    public ISnackbar Snackbar { get; set; }
    private CodeTaskList? _taskListRef;


    private async Task OnAddTaskClicked()
    {
        var newCodeTask = new CodeTask()
        {
            Title = string.Empty,
            Description = string.Empty,
            DefaultCode = "# write your python code here",
            Examples = [],
            Constraints = [],
            Version = 1,
            Difficulty = CodeTaskDifficulty.Beginner,
            CurrencyReward = 10,
            // I'll set these fore initialisation purposes, but they will be updated server-side anyways 
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var parameters = new DialogParameters<AddCodeTaskDialog> { { x => x.CodeTask, newCodeTask } };
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Medium, CloseButton = true };
        
        var dialog = await DialogService.ShowAsync<AddCodeTaskDialog>("Add Code Task", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CodeTask updatedTask) 
        {
            var updateCodeTask = await CodeTasksService.AddOrUpdateCodeTask(updatedTask);
            if (updateCodeTask is not null)
            {
                Snackbar.Add("Successfully added code task", Severity.Success);
                _taskListRef?.AddTaskToList(updateCodeTask);
            }
        }
    }
}