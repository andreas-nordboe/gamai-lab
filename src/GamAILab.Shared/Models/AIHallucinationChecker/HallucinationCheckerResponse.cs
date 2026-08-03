namespace GamAILab.Shared.Models.AIHallucinationChecker;

public class HallucinationCheckerResponse
{
    public  bool IsConsistent { get; init; }
    public required string Summary { get; init; }
    public required List<string> ConflictedClaims { get; init; }
}