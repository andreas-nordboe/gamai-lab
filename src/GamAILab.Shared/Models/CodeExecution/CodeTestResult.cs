using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.CodeExecution;

public sealed class CodeTestResult
{
    [Key]
    public int Key { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("passed")]
    public bool Passed { get; init; }
    [JsonPropertyName("expectedResult")]
    [NotMapped] // TODO this may have to be changed later for persistence
    public JsonElement? ExpectedResult { get; init; }
    [JsonPropertyName("actualResult")]
    [NotMapped] // TODO this may have to be changed later for persistence
    public JsonElement? ActualOutput { get; init; }
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}