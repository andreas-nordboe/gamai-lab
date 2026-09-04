using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AICodeEvaluation.Hints;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;
using GamAILab.Shared.Models.Game.DTOs;

namespace GamAILab.Frontend.Client.Services;

public interface ICodeSubmissionService
{
    Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, CancellationToken cancellationToken = default);
    Task<CodeExecutionResponse> ExecuteCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<AICodeHintResponse> GetAICodeHintAsync(AICodeHintRequest codeSubmission, CancellationToken cancellationToken = default);
    Task<CodeTaskLearnerProgress> GetCodeTaskProgressAsync(int codeTaskId, CancellationToken cancellationToken = default);

}