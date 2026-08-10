using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AICodeEvaluation;

public sealed class AICodeEvaluationTest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("functionName")]
    public required string FunctionName { get; init; } // TODO extend this later to be varied types of inputs (not just functions)
    [JsonPropertyName("arguments")]
    public required List<int> Arguments { get; set; }
    [JsonPropertyName("expectedResult")]
    public required string ExpectedResult { get; init; }

    public ExpectedResultType ExpectedResultType { get; set; }
}