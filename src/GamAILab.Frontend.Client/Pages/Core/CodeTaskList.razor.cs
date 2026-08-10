using GamAILab.Frontend.Client.Dialogs;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTaskList : ComponentBase
{
    [Parameter] 
    public List<CodeTask?> CodeTasks { get; set; } = new List<CodeTask?>();
    [Inject] public ICodeTasksService CodeTasksService { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    private bool _isLoadingCodeTasks;
    [Parameter] public bool CanModifyTasks { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }
    
    [CascadingParameter(Name = "OnTaskAdded")]
    protected Action<CodeTask>? OnTaskAdded { get; set; }    
    protected override async Task OnInitializedAsync()
    {
        if (OnTaskAdded is not null)
        {
            OnTaskAdded = (newTask) => 
            {
                CodeTasks.Add(newTask);
                StateHasChanged();
            };
        }

        var codeTasks = await CodeTasksService.ListCodeTasksAsync();
        if (codeTasks.Any())
        {
            CodeTasks = codeTasks;
        }
        
    }

    private void OnTaskClicked(CodeTask task)
    {
        NavigationManager.NavigateTo($"code-task/{task.Id}");
    }

    private async Task OnDeleteTaskClicked(int codeTaskId)
    {
        bool? result = await DialogService.ShowMessageBoxAsync(
            "Warning: Delete code task?", 
            "Deleting can not be undone!", 
            yesText:"Delete", cancelText:"Cancel");

        if (result is not null)
        {
            var deletedCodeTask = await CodeTasksService.DeleteCodeTask(codeTaskId);
            if (deletedCodeTask)
            {
                var codeTaskToRemove = CodeTasks.Where(codeTask => codeTask?.Id == codeTaskId).FirstOrDefault();
                if (codeTaskToRemove is not null)
                {
                    CodeTasks.Remove(codeTaskToRemove);
                    StateHasChanged();
                    Snackbar.Add("Successfully deleted code task", Severity.Success);
                }
            }
            else
            {
                Snackbar.Add("Failed to delete code task.", Severity.Error);
            }
        }
    }
    
    private async Task EditCodeTaskClicked(CodeTask codeTask)
    {
        var parameters = new DialogParameters<AddCodeTaskDialog>
        {
            { x => x.CodeTask, codeTask  },
            { x => x.IsEditing, true }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Medium, CloseButton = true };
        
        var dialog = await DialogService.ShowAsync<AddCodeTaskDialog>($"Edit Code Task with ID: {codeTask.Id}", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CodeTask updatedTask) 
        {
            var updateCodeTask = await CodeTasksService.AddOrUpdateCodeTask(updatedTask);
            if (updateCodeTask is not null)
            {
                Snackbar.Add("Successfully edited code task", Severity.Success);
            }
        }
    }

    public void AddTaskToList(CodeTask newTask)
    {
        CodeTasks.Add(newTask);
        StateHasChanged();
    }
}