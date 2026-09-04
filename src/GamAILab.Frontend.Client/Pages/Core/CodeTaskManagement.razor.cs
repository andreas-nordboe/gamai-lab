using System.Text.Encodings.Web;
using System.Text.Json;
using GamAILab.Frontend.Client.Dialogs;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    private CodeTaskList? _taskListRef;


    private async Task OnAddTaskClicked()
    {
        // Setup code defaults
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
    
    private async Task OnDownloadTasksClicked()
    {
        // Fetch again to get a fresh list that includes 
        var codeTasks = await CodeTasksService.ExportCodeTasksAsync();
        if (codeTasks.Any())
        {
            var json = JsonSerializer.Serialize(codeTasks, new JsonSerializerOptions 
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // fixes malformed characters issue (like ')
            });
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await using var stream = new MemoryStream(bytes);
            using var streamReference = new DotNetStreamReference(stream);
            
            await JSRuntime.InvokeVoidAsync("downloadFile", $"code-tasks.json", streamReference);
        }
    }
    
    // Downloads verified code evaluation examples (different format that has less information for the hallucination checker)
    private async Task OnDownloadCodeEvaluationExamplesClicked()
    {
        // Fetch again to get a fresh list that includes 
        var codeTasks = await CodeTasksService.ExportVerifiedCodeTaskExamplesAsync();
        if (codeTasks.Any())
        {
            var json = JsonSerializer.Serialize(codeTasks, new JsonSerializerOptions 
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // fixes malformed characters issue (like ')
            });
            
            Console.WriteLine(json);
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await using var stream = new MemoryStream(bytes);
            using var streamReference = new DotNetStreamReference(stream);
            
            await JSRuntime.InvokeVoidAsync("downloadFile", $"verified-code-evaluations.json", streamReference);
        }
    }
    
    private async Task OnGenerateCodeTaskClicked()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Medium, CloseButton = true };
        
        var dialog = await DialogService.ShowAsync<GenerateCodeTaskDialog>("Generate Code Task", options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CodeTask codeTask) 
        {
            var generatedCodeTask = await CodeTasksService.AddOrUpdateCodeTask(codeTask);
            if (generatedCodeTask is not null)
            {
                _taskListRef?.AddTaskToList(generatedCodeTask);
                Snackbar.Add("Successfully generated and saved code task", Severity.Success);
                StateHasChanged();
            }
        }
        
    }
}