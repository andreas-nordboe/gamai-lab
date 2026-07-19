using GamAILab.Shared.Models;

namespace GamAILab.Frontend.Client.Services;

public interface ICodeTasksService
{
    Task <List<CodeTask?>> ListCodeTasksAsync();
    Task<CodeTask> GetCodeTaskAsync(int codeTaskId);
}