using GamAILab.Shared.Models;

namespace GamAILab.WebApi.Services.CodeTasks;

public interface ICodeTaskService
{
    public Task AddCodeTask(CodeTask codeTask);
    public Task<CodeTask?> GetCodeTaskById(int codeTaskId);
    public Task<List<CodeTask>> GetAllCodeTasks();
    public Task<bool> DeleteCodeTaskById(int codeTaskId);
    public Task SeedCodeTasks();
}