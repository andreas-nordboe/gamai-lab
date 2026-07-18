using GamAILab.Frontend.Client.Components.CodeTasks;
using GamAILab.Frontend.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTasks : ComponentBase
{
   [Inject] public NavigationManager NavigationManager { get; set; }
   [Inject] public ICodeSubmissionService CodeSubmissionService { get; set; }
   [Inject] public ISnackbar Snackbar { get; set; }
   
   private CodeEditorPanel? _codeEditorPanel;
   private CodeEditorPanel? _codeOutputPanel;
   private bool _codeIsExecuting;
   private string _codeExecutionOutput = string.Empty;
   
   private async Task RunCode()
   {
      if (_codeEditorPanel is null || _codeIsExecuting)
      {
         Snackbar.Add("Failed to run code", Severity.Error);
         return;
      }

      try
      {
         _codeIsExecuting = true;
         _codeExecutionOutput = "Running code...";
         var code = await _codeEditorPanel.GetCodeAsync();

         if (string.IsNullOrEmpty(code))
         {
            Snackbar.Add("Please write code before running", Severity.Error);
            return;
         }
         
         var codeExecution = await CodeSubmissionService.ExecuteCodeAsync(code);

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
   
   private void ResetCode()
   {
      // TODO   
   }
   
   private void GetCodeHint()
   {
      // TODO   
   }
   
   private void SubmitCode()
   {
      // TODO   
      // This is just for testing the layout temporarily (a modal might be more appropriate later) 
      NavigationManager.NavigateTo("code-output");
   }
   
   
}