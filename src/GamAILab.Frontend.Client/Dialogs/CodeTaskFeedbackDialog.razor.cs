using System.Text.Json;
using GamAILab.Frontend.Client.Components.CodeTasks;
using GamAILab.Shared.Models.AICodeEvaluation;
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
    
    [Parameter] public string? HallucinationCheckerResult { get; set; }
    
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
}