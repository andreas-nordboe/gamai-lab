using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamAILab.Shared.Models.Game.DTOs;

[NotMapped]
public class AchievementRequest
{
    [Key]
    public string AchievementId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}