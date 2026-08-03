using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.AIHallucinationChecker;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.WebApi.Services.HallucinationChecker;

public interface IHallucinationCheckerService
{
    Task<HallucinationCheckResult> CheckAIFeedbackConsistencyAsync(CodeTask codeTask, CodeSubmission codeSubmission,
        AICodeEvaluationPlan codeEvaluationPlan, CodeExecutionResult executionResult,
        AICodeTaskFeedback aiCodeTaskFeedback, CancellationToken cancellationToken = default);
}