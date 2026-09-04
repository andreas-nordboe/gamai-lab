using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.Game;

// The visuals can be mapped to ID from within Unreal Engine
public class Achievement
{
    [Key] 
    public int Id { get; set; }
    public string AchievementId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
   public DateTime? GrantedAt { get; set; }
}