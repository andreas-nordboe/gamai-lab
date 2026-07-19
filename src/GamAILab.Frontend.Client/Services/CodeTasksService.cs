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
}