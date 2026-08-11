using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GamAILab.Shared.Models;

namespace GamAILab.Frontend.Client.Services;

public class CodeTasksService : ICodeTasksService
{
    private readonly HttpClient _httpClient;

    public CodeTasksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CodeTask>> ListCodeTasksAsync()
    {
         return await _httpClient.GetFromJsonAsync<List<CodeTask>>("api/code-tasks/tasks") ?? [];
    }

    public async Task<CodeTask?> GetCodeTaskAsync(int codeTaskId)
    {
        var codeTask = await _httpClient.GetAsync($"api/code-tasks/{codeTaskId}");
        if (codeTask.StatusCode == HttpStatusCode.NotFound || !codeTask.IsSuccessStatusCode)
            return null;
        
        return await codeTask.Content.ReadFromJsonAsync<CodeTask>();
    }
    
    public async Task<bool> DeleteCodeTask(int codeTaskId)
    {
        var codeTask = await _httpClient.DeleteAsync($"api/code-tasks/delete/{codeTaskId}");
        if (codeTask.StatusCode == HttpStatusCode.NotFound || !codeTask.IsSuccessStatusCode)
            return false;
        
        return await codeTask.Content.ReadFromJsonAsync<bool>();
    }
    
    public async Task<CodeTask?> AddOrUpdateCodeTask(CodeTask codeTask)
    {
        var addOrUpdateCodeTask = await _httpClient.PostAsJsonAsync($"api/code-tasks/add-or-update", codeTask);
        if (!addOrUpdateCodeTask.IsSuccessStatusCode)
            return null;
        
        return await addOrUpdateCodeTask.Content.ReadFromJsonAsync<CodeTask>();
    }
    
    
    public async Task<CodeTask?> ReGenerateCodeEvaluationPlanAsync(int codeTaskId)
    {
        // It looks a bit unconventional to use a query parameter and sending an empty body, however PUT is still also more appropriate for this operation
        var response = await _httpClient.PutAsync($"api/code-tasks/re-generate-code-evaluation-plan/{codeTaskId}", null);
        if (!response.IsSuccessStatusCode)
            return null;
        
        return await response.Content.ReadFromJsonAsync<CodeTask>();
    }
}