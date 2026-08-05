using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.Frontend.Client.Services;

public interface ICodeSubmissionService
{
    Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, CancellationToken cancellationToken = default);
    Task<CodeExecutionResponse> ExecuteCodeAsync(string code, CancellationToken cancellationToken = default);
}