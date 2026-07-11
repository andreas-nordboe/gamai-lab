using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace GamAILab.WebApi.Services.LLMService;

public class LLMService : ILLMService
{
    private readonly OllamaApiClient _client;
    private readonly ILogger<LLMService> _logger;

    public LLMService(HttpClient httpClient, IConfiguration configuration, ILogger<LLMService> logger)
    {
        _logger = logger;
        var model = configuration.GetValue<string>("Ollama:Model")!;
        _client = new OllamaApiClient(httpClient, model);
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("The prompt cannot be empty", nameof(prompt));
        }
        
        var result = new StringBuilder();

        await foreach (var token in _client.GenerateAsync(prompt, cancellationToken: cancellationToken))
        {
            result.Append(token?.Response);
        }

        return result.ToString();
    }
    
    public async Task<string> ChatAsync(ChatRequest chatRequest, CancellationToken cancellationToken = default)
    {
        var responseBuilder = new StringBuilder();

        await foreach (var response in _client.ChatAsync(chatRequest, cancellationToken: cancellationToken))
        {
            var content = response?.Message.Content;

            if (!string.IsNullOrWhiteSpace(content))
            {
                _logger.LogDebug($"LLM response chunk: {response}");
                
                if(response?.Message.Content is { Length: > 0 } contentMessage)
                    responseBuilder.Append(content);
            }
        }

        if (responseBuilder.Length == 0)
        {
            throw new InvalidOperationException(
                $"LLM returned no message. Model {chatRequest.Model}");
        }
        
        var responseString = responseBuilder.ToString();
        return responseString;
    }
}