using System.Text.Json;
using System.Text.Json.Serialization;
using GamAILab.Shared.Models.CodeExecution;

namespace GamAILab.Shared.Models.AICodeEvaluation;

// This uses JsonPropertyNames because the code runner is written in Python and it requires proper casing for schemas/validation
public sealed class AICodeEvaluationTest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("testType")]
    public required CodeTestType TestType { get; init; }
    
    [JsonPropertyName("arguments")]
    public required List<CodeTestValue> Arguments { get; init; } = [];

    [JsonPropertyName("standardInput")] 
    public required List<string> StandardInput { get; init; } = [];

    [JsonPropertyName("expectedResult")]
    public required string? ExpectedResult { get; init; }

    [JsonPropertyName("expectedResultType")]
    public ExpectedResultType ExpectedResultType { get; set; }
    
    [JsonPropertyName("functionName")]
    public string? FunctionName { get; set; }
    
    [JsonPropertyName("exception")]
    public string? Exception { get; set; }
}