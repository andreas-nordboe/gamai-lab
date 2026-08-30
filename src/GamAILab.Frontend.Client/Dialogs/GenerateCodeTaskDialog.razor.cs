using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class GenerateCodeTaskDialog : ComponentBase
{
    [Parameter] 
    public string CodeTaskDescription { get; set; }
    [Inject] 
    public ICodeTasksService CodeTasksService { get; set; } = default!;

    public string _codeTaskDescription { get; set; }
    public string _codeTaskGameStory { get; set; }
    private CodeTask? _generatedCodeTask;
    private bool _isGeneratingCodeTask;
    
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private async Task OnGenerateCodeTaskClicked()
    {
        try
        {
            _isGeneratingCodeTask = true;
            
            _generatedCodeTask = await CodeTasksService.GenerateCodeTaskAsync(new GenerateCodeTaskRequest
            {
                Description = CodeTaskDescription,
                GameStory = _codeTaskGameStory,
            });
        }
        finally
        {
            _isGeneratingCodeTask = false;
        }
    }
    
    private async Task OnSaveGeneratedCodeTaskClicked()
    {
        if (_generatedCodeTask is not null)
        {
            MudDialog.Close(DialogResult.Ok(_generatedCodeTask));
        }
    }
    
    private void Cancel() => MudDialog.Cancel();
}