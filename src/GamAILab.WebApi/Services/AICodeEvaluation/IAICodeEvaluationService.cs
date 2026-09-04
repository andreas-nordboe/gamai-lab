using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;

namespace GamAILab.WebApi.Services;

public interface IAICodeEvaluationService
{
    Task<AICodeEvaluationPlan> GenerateEvaluationPlanAsync(CodeTask codeTaskContext, CancellationToken cancellationToken = default);
}