using System.Text.Json;
using GamAILab.Frontend.Client.Components.CodeTasks;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.CodeSubmission;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Dialogs;

public partial class CodeTaskFeedbackDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Parameter]
    public CodeSubmissionResult CodeSubmissionFeedback { get; set; }
    
    private CodeEditorPanel? _codeFeedbackPanel;
    
    private string CodeExecutionDuration => $"{CodeSubmissionFeedback.ExecutionDuration.TotalMilliseconds:N0} ms";
    private int PassedTests => CodeSubmissionFeedback.CodeExecution.CodeTests.Count(tests => tests.Passed);
    private int TotalTests => CodeSubmissionFeedback.CodeExecution.CodeTests.Count();

    private IReadOnlyList<string> FeedbackEvidence
    {
        get
        {
            var evidenceJson = CodeSubmissionFeedback.AIFeedback.CodeTaskExecutionEvidence;
            if (string.IsNullOrWhiteSpace(evidenceJson))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<string>>(evidenceJson) ?? [];
            }
            catch (JsonException)
            {
                return [evidenceJson];
            }
        }
    }

    private static string FormatJsonValue(JsonElement? value)
    {
        if (value is null || value.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return "Not returned";

        return value.Value.ValueKind == JsonValueKind.String ? value.Value.GetString() ?? string.Empty : value.Value.GetRawText();
    }

    private Color FormatOutcomeColor()
    {
        switch (CodeSubmissionFeedback.AIFeedback.TaskOutcome)
        {
            case CodeTaskOutcome.Correct:
                return Color.Success;
            case CodeTaskOutcome.Incorrect:
                return Color.Error;
            case CodeTaskOutcome.ExecutionError:
                return Color.Error;
            case CodeTaskOutcome.Partial:
                return Color.Warning;
            default:
                return Color.Default;
            
        }
    }

    private void LogOut () => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
    
    private static Severity GetTaskOutcomeServerity(CodeTaskOutcome status)
    {
        switch (status)
        {
            case CodeTaskOutcome.Correct:
                return Severity.Success;
            case CodeTaskOutcome.Incorrect:
                return Severity.Error;
            case CodeTaskOutcome.ExecutionError:
                return Severity.Error;
            case CodeTaskOutcome.Partial:
                return Severity.Warning;
            default:
                return Severity.Error;
        }
    }
    
    private static Severity GetVerificationSeverity(HallucinationCheckerStatus status)
    {
        switch (status)
        {
            case HallucinationCheckerStatus.IsConsistent:
                return Severity.Success;
            case HallucinationCheckerStatus.IsNotConsistent:
                return Severity.Error;
            case HallucinationCheckerStatus.Unverifiable:
                return Severity.Warning;
            default:
                return Severity.Info;
        }
    }

    private static string GetHallucinationStatusTitle(HallucinationCheckerStatus status)
    {
        switch (status)
        {
            case HallucinationCheckerStatus.IsConsistent:
                return "The feedback is consistent with the code execution";
            case HallucinationCheckerStatus.IsNotConsistent:
                return "The feedback is not consistent with the code execution";
            case HallucinationCheckerStatus.Unverifiable:
                return "The feedback was unverifiable";
            default:
                return "Verification status is invalid";
        }
    }

    private static string GetHallucinationStatusText(HallucinationCheckerStatus status)
    {
        switch (status)
        {
            case HallucinationCheckerStatus.IsConsistent:
                return "Consistent";
            case HallucinationCheckerStatus.IsNotConsistent:
                return "Not consistent";
            case HallucinationCheckerStatus.Unverifiable:
                return "Unverifiable";
            default:
                return "Verification status is invalid";
        }
    }
}