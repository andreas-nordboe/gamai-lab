using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.CodeExecution;

namespace GamAILab.WebApi.Services.CodeExecution;

public interface ICodeExecutionService
{
    Task<CodeTestResult> ExecuteCodeAsync(string learnerCode, AICodeEvaluationPlan codeEvaluationPlan, CancellationToken cancellationToken = default);
}