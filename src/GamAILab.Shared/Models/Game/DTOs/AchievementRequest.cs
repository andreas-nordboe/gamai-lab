using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.Game.DTOs;

public class AchievementRequest
{
    [Key]
    public string AchievementId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}