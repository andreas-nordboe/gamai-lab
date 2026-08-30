using System.Text.Json;
using Blazored.LocalStorage;
using GamAILab.Frontend.Client.Components.CodeTasks;
using GamAILab.Frontend.Client.Dialogs;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AICodeEvaluation.DTOs;
using GamAILab.Shared.Models.AICodeEvaluation.Hints;
using GamAILab.Shared.Models.CodeSubmission;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTaskPage : ComponentBase
{
   // Services
   [Inject] 
   public NavigationManager NavigationManager { get; set; }
   [Inject] 
   public ICodeSubmissionService CodeSubmissionService { get; set; }
   [Inject] 
   public ISnackbar Snackbar { get; set; }
   [Inject] 
   public ICodeTasksService CodeTasksService { get; set; } = default;
   [Inject] 
   public IDialogService DialogService { get; set; }
   [Inject] 
   public ILocalStorageService _localStorage { get; set; }
   
   // Realtime status updates
   private HubConnection? _webSocketConnection;
   private CodeEvaluationStatus? _codeEvaluationStatus;
   
   // Hint system
   private string _helpInput { get; set; }
   private List<AICodeHintChatLog> _helpChatLogs { get; set; } = [];
   
   // Panels
   private CodeEditorPanel? _codeEditorPanel;
   private CodeEditorPanel? _codeOutputPanel;
   
   // States
   private bool _codeIsExecuting;
   private bool _codeIsSubmitting;
   private string _codeExecutionOutput = string.Empty;
   private bool _isLoading = true;
   private bool _showCodeAssistant = false;
   private bool _isLoadingHint = false;
   private int _attemptsUsed;
   private int _hintsUsed;
   
   // Task related vars
   [Parameter] public int TaskId { get; set; }
   public CodeTask? _codeTask { get; set; }

   protected override async Task OnInitializedAsync()
   {
      try
      {
         _codeTask = await CodeTasksService.GetCodeTaskAsync(TaskId);


         if (_codeTask is null)
         {
            return;
         }
         
         var progress = await CodeSubmissionService.GetCodeTaskProgressAsync(TaskId);

         _attemptsUsed = progress.Attempts;
         _hintsUsed = progress.HintsUsed;
         _helpChatLogs = progress.ChatLogs;
         StateHasChanged();
            
         // Connect to websocket for real-time status updates 
         _webSocketConnection = new HubConnectionBuilder().WithUrl("http://localhost:5270/hubs/code-evaluation", options => { options.AccessTokenProvider = async () => await _localStorage.GetItemAsync<string>("authToken"); })
            .WithAutomaticReconnect().Build();
            
         _webSocketConnection.On<CodeEvaluationStatus>(
            "CodeEvaluationStatusChanged",
            status =>
            {
               if (status.CodeTaskId != TaskId)
                  return;

               _codeEvaluationStatus = status;
               InvokeAsync(StateHasChanged);
            });

         await _webSocketConnection.StartAsync();
         
         Console.WriteLine($"Code evaluation SignalR status: {_webSocketConnection.State}");
      }
      catch (Exception e)
      {
         Snackbar.Add($"Failed to load code task: {e.Message}", Severity.Error);
      }
      finally
      {
         _isLoading = false;
      }

      try
      {
         if (_codeEditorPanel is not null && _codeTask is not null)
         {
            await _codeEditorPanel.SetCodeAsync(_codeTask.DefaultCode);
         }
      }
      catch (Exception e)
      {
         Console.WriteLine(e);
      }
   }

   private async Task RunCodeAsync()
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
      await _codeEditorPanel?.SetCodeAsync(_codeTask?.DefaultCode); 
   }
   
   private void ToggleGetCodeAssistance()
   {
      _showCodeAssistant = !_showCodeAssistant;
   }
   
   private async Task SubmitCodeAsync()
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
            CodeAttempt =  learnerCode
         };

         var submitCodeResponse = await CodeSubmissionService.SubmitCodeAsync(codeSubmissionRequest);
         _attemptsUsed = submitCodeResponse.AttemptNumber;
         _codeIsSubmitting = false;
         if (_codeIsExecuting)
         {
            _codeIsExecuting = false;
         }
         StateHasChanged();
         
         var parameters = new DialogParameters<CodeTaskFeedbackDialog>
         {
            { x => x.CodeSubmissionFeedback, submitCodeResponse }
         };
         
         var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.Large, CloseButton = true };
         var dialog = await DialogService.ShowAsync<CodeTaskFeedbackDialog>("Code Task Feedback", parameters, options);
         var result = await dialog.Result;

         if (result != null && !result.Canceled)
         {
            // TODO
         }
         
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

   private async Task OnSendHelpMessageClicked()
   {
      if (_isLoadingHint || string.IsNullOrWhiteSpace(_helpInput) || _codeEditorPanel is null)
      {
         return;
      }

      var learnerQuestion = _helpInput.Trim();
      _helpInput = string.Empty;

      try
      {
         _isLoadingHint = true;

         var learnerCode = await _codeEditorPanel.GetCodeAsync();

         var conversationForRequest = _helpChatLogs.ToList();

         _helpChatLogs.Add(new AICodeHintChatLog
         {
            ChatLogRole = AICodeHintChatLogRole.Learner,
            Content = learnerQuestion
         });

         StateHasChanged();
         
         // Retrieve or run code execution on behalf of the learner
         // this was changed as it could become frustrating for users to see their code being run every time
         // if (string.IsNullOrWhiteSpace(_codeExecutionOutput))
         // {
         //    await RunCodeAsync();
         // }

         var request = new AICodeHintRequest
         {
            CodeTaskId = TaskId,
            LearnerCode = learnerCode,
            Question = learnerQuestion,
            //LastCodeExecutionOutcome = _codeExecutionOutput, // this is now running on from the backend instead, a accuracy over performance trade-off for now.. 
            ChatLogs = conversationForRequest
         };

         var response = await CodeSubmissionService.GetAICodeHintAsync(request);

         _helpChatLogs.Add(new AICodeHintChatLog
         {
            ChatLogRole = AICodeHintChatLogRole.AIAssistant,
            Content = response.Message
         });
         
         _hintsUsed++;
         StateHasChanged();
      }
      catch (Exception e)
      {
         Snackbar.Add($"AI Assistant hint request failed: {e.Message}", Severity.Error);
      }
      finally
      {
         _isLoadingHint = false;
      }
   }
   
   private static string GetCodeEvaluationStepTitle(CodeEvaluationStep codeEvaluationStep)
   {
      return codeEvaluationStep switch
      {
         CodeEvaluationStep.SubmissionInitiated => "Submission received",
         CodeEvaluationStep.ExecutingCode => "Executing code",
         CodeEvaluationStep.GeneratingAIFeedback => "AI is evaluating your code and generating feedback",
         CodeEvaluationStep.RunningHallucinationChecker => "AI hallucination checker is verifying the code evaluation and feedback consitency",
         CodeEvaluationStep.UpdatingGameProgress => "Updating game progress",
         CodeEvaluationStep.Finished => "Evaluation completed",
         _ => "Waiting"
      };
   }
   
   /*private static MudBlazor.Color GetCodeEvaluationStepColour(CodeEvaluationStep? codeEvaluationStep)
   {
      return codeEvaluationStep switch
      {
         CodeEvaluationStep.SubmissionInitiated => Color.Default,
         CodeEvaluationStep.ExecutingCode => Color.Info,
         CodeEvaluationStep.GeneratingAIFeedback => Color.Primary,
         CodeEvaluationStep.RunningHallucinationChecker => Color.Warning,
         CodeEvaluationStep.UpdatingGameProgress => Color.Secondary,
         CodeEvaluationStep.Finished => Color.Success,
         _ => Color.Default
      };
   }*/
   
}