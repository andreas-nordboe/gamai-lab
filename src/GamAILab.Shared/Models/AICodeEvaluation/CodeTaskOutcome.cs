using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AICodeEvaluation;

[JsonConverter(typeof(JsonStringEnumConverter<CodeTaskOutcome>))]
public enum CodeTaskOutcome
{
    Incorrect,
    Partial,
    Correct,
    ExecutionError
}

