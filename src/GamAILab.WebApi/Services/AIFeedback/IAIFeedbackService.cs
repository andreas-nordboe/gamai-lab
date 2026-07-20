using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.WebApi.Services;

public interface IAIFeedbackService
{
    Task<AICodeTaskFeedback> GenerateCodeTaskFeedbackAsync(CodeTask codeTask, CodeSubmission submission,
        AICodeEvaluationPlan codeEvaluationPlan, CodeExecutionResult executionResult,
        CancellationToken cancellationToken = default);
}