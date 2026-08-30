using GamAILab.Shared.Models;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.DTOs;

namespace GamAILab.Frontend.Client.Services;

public interface ICodeTasksService
{
    Task <List<CodeTask?>> ListCodeTasksAsync();
    Task<CodeTask> GetCodeTaskAsync(int codeTaskId);
    Task<bool> DeleteCodeTask(int codeTaskId);
    Task<CodeTask?> AddOrUpdateCodeTask(CodeTask codeTask);
    Task<CodeTask?> ReGenerateCodeEvaluationPlanAsync(int codeTaskId);
    Task<CodeTask?> GenerateCodeTaskAsync(GenerateCodeTaskRequest aiPersonaDescription);
    Task<List<CodeTask>> ExportCodeTasksAsync();
    Task<List<VerifiedCodeEvaluationExample>> ExportVerifiedCodeTaskExamplesAsync();
}