using System.Text.Json;
using GamAILab.Frontend.Client.Components.CodeTasks;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.CodeSubmission;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTaskPage : ComponentBase
{
   // Services
   [Inject] public NavigationManager NavigationManager { get; set; }
   [Inject] public ICodeSubmissionService CodeSubmissionService { get; set; }
   [Inject] public ISnackbar Snackbar { get; set; }
   [Inject] public ICodeTasksService CodeTasksService { get; set; } = default;
   
   // Panels
   private CodeEditorPanel? _codeEditorPanel;
   private CodeEditorPanel? _codeOutputPanel;
   
   // States
   private bool _codeIsExecuting;
   private bool _codeIsSubmitting;
   private string _codeExecutionOutput = string.Empty;
   private bool _isLoading = true;
   
   // Task related vars
   [Parameter] public int TaskId { get; set; }
   public CodeTask? _codeTask { get; set; }

   protected override async Task OnInitializedAsync()
   {
      try
      {
         _codeTask = await CodeTasksService.GetCodeTaskAsync(TaskId);
         if (_codeTask is not null)
         {
            await _codeEditorPanel.SetCodeAsync(_codeTask.DefaultCode);
            StateHasChanged();
         }
      }
      catch (Exception e)
      {
         _isLoading = false;
      }
   }

   private async Task RunCode()
   {
      if (_codeEditorPanel is null || _codeIsExecuting)
      {
         Snackbar.Add("Failed to run code", Severity.Error);
         return;
      }

      try
      {
         var codeInput = await _codeEditorPanel.GetCodeAsync();
         
         _codeIsExecuting = true;
         _codeExecutionOutput = "Running code...";
         StateHasChanged();
         
         if (string.IsNullOrEmpty(codeInput))
         {
            Snackbar.Add("Please write code before running", Severity.Error);
            return;
         }
         
         var codeExecution = await CodeSubmissionService.ExecuteCodeAsync(codeInput);

         if (codeExecution.TimedOut)
         {
            _codeExecutionOutput = string.IsNullOrWhiteSpace(codeExecution.CodeError) ? "Running code timed out" : codeExecution.CodeError;
            Snackbar.Add("Running code timed out", Severity.Error);
         }
         else if (!codeExecution.DidComplete)
         {
            _codeExecutionOutput = string.IsNullOrWhiteSpace(codeExecution.CodeError) ? "Running code failed" : codeExecution.CodeError;
         }
         else
         {
            // This should work for now I guess
            _codeExecutionOutput = string.IsNullOrWhiteSpace(codeExecution.CodeOutput) ? "Code returned no output" : codeExecution.CodeOutput;
         }
      }
      catch (Exception e)
      {
         _codeExecutionOutput = e.Message;
      }
      finally
      {
         _codeIsExecuting = false;
         
         if (_codeOutputPanel != null) 
            await _codeOutputPanel.SetCodeAsync(_codeExecutionOutput);
      }
      
   }
   
   private async Task ResetCode()
   {
      await _codeEditorPanel?.SetCodeAsync(string.Empty); 
   }
   
   private void GetCodeHint()
   {
      // TODO   
   }
   
   private async Task SubmitCode()
   {
      if (_codeEditorPanel is null || _codeIsSubmitting)
      {
         Snackbar.Add("Failed to submit code", Severity.Error);
         return;
      }

      try
      {
         _codeIsSubmitting = true;

         var learnerCode = await _codeEditorPanel?.GetCodeAsync()!;
         if (string.IsNullOrWhiteSpace(learnerCode))
         {
            Snackbar.Add("Please provide code before pressing submit", Severity.Error);
         }

         var codeSubmissionRequest = new CodeSubmissionRequest
         {
            CodeTaskId =  TaskId,
            Code =  learnerCode
         };

         var submitCodeResponse = await CodeSubmissionService.SubmitCodeAsync(codeSubmissionRequest);
      }
      catch (Exception e)
      {
         // TODO, handle HTTP inside code submission service first
      }
      finally
      {
         _codeIsSubmitting = false;
      }
      
   }
   
   
}