using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.Game.DTOs;

public class LearnerGameProgressRequest
{
    [Key] 
    public int Id { get; set; }
    public int Level { get; set; }
    public int Currency { get; set; } // TODO there could eventually be several currency types
    public List<AchievementRequest>? Achievements { get; set; } = [];
    // TODO I'll potentially add rewards/owned items here
}