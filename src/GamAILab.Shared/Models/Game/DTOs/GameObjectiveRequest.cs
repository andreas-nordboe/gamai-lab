namespace GamAILab.Shared.Models.Game.DTOs;

public class GameObjectiveRequest
{
    public string? ObjectiveId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    // For completing something x a set amount of times (1/4)
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
}