using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AICodeEvaluation;

// Returned by the LLM
public class AICodeFeedbackResponse
{
    [JsonPropertyName("outcome")]
    public required string TaskOutcome { get; init; }
    [JsonPropertyName("explanation")]
    public required string Explanation { get; init; }
    [JsonPropertyName("hint")]
    public string? Hint { get; init; }
    [JsonPropertyName("evidence")]
    public required IReadOnlyList<string> CodeTaskExecutionEvidence { get; init; }
}