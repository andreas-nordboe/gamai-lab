using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.Game;

// This will be a monolithic model for now but it would 
// be better to separate these into different models later for better scalability
public class LearnerGameProgress
{
    [Key]
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int Level { get; set; }
    public int Currency { get; set; } // TODO there could eventually be several currency types
    public List<Achievement> Achievements { get; set; } = [];
    public DateTime LastUpdated { get; set; }
    public List<CodeTask> CompletedTasks { get; set; } = [];
    // TODO I'll potentially add rewards/owned items here
}