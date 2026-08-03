namespace GamAILab.Shared.Models.AIHallucinationChecker;

// This is just used within the Hallucination checker service itself, and the DTO is used for the API
public class HallucinationCheckerResponse
{
    public  bool IsConsistent { get; init; }
    public required string Summary { get; init; }
    public required List<string> ConflictedClaims { get; init; }
}