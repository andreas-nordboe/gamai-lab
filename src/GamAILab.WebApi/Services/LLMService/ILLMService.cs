using OllamaSharp.Models.Chat;

namespace GamAILab.WebApi.Services.LLMService;

public interface ILLMService
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> ChatAsync(ChatRequest chatRequest, CancellationToken cancellationToken = default);
}