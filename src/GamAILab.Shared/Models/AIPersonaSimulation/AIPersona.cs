using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GamAILab.Shared.Models.AIPersonaSimulation;

public class AIPersona
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Background { get; set; } // more "personalised" details such as age, country, emotional
    public List<string> LearningCapabilities { get; set; }
    public List<string> LearningDifficulties { get; set; }
    public List<string> AccessibilityRequirements { get; set; }
    public CodeTaskDifficulty AssignedDifficulty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // TODO learning capabilities (previous skills), background and difficulties, traits?
}