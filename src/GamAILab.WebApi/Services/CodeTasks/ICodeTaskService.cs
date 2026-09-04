using GamAILab.Shared.Models;
using GamAILab.Shared.Models.DTOs;

namespace GamAILab.WebApi.Services.CodeTasks;

public interface ICodeTaskService
{
    public Task AddCodeTask(CodeTask codeTask, bool generateCodeEvaluationPlan = true);
    public Task<CodeTask?> GetCodeTaskById(int codeTaskId);
    public Task<List<CodeTask>> GetAllCodeTasks();
    public Task<bool> DeleteCodeTaskById(int codeTaskId);
    public Task<List<CodeTask>> SeedCodeTasks();
    public Task<CodeTask> AddOrUpdateCodeTask(CodeTask codeTask);
    public Task<CodeTask?> ReGenerateCodeEvaluationPlanAsync(int codeTaskId);
    public Task<CodeTask?> GenerateCodeTaskAsync(GenerateCodeTaskRequest generateCodeTaskRequest, CancellationToken cancellationToken = default);
}