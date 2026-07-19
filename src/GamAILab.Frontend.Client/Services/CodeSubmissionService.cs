using System.Net.Http.Json;
using GamAILab.Shared.Models.CodeExecution;
using GamAILab.Shared.Models.CodeSubmission;

namespace GamAILab.Frontend.Client.Services;

public class CodeSubmissionService : ICodeSubmissionService
{
    private readonly HttpClient _httpClient;

    public CodeSubmissionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CodeSubmissionResult> SubmitCodeAsync(CodeSubmissionRequest codeSubmission, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync<CodeSubmissionRequest>("api/code-submission/submit", codeSubmission, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            // TODO parse HTTP error codes 
            throw new HttpRequestException("An internal error occured");
        }
        
        var responseContent = await response.Content.ReadFromJsonAsync<CodeSubmissionResult>(cancellationToken: cancellationToken);
        
        return responseContent;
    }

    public async Task<CodeExecutionResponse> ExecuteCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var request = new CodeExecutionRequest()
        {
            Code = code,
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/code-execution/execute", request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Invalid or empty password");
            
            throw new HttpRequestException("An internal error occured");
        }
        
        return await response.Content.ReadFromJsonAsync<CodeExecutionResponse>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("The API returned an empty code execution result");
    }
}