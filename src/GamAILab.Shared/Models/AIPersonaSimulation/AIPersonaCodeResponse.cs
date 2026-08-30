using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.AIPersonaSimulation;

public class AIPersonaCodeResponse
{
    [Key]
    public int Id { get; set; }
    public string Code { get; set; }
    public List<string> Struggles { get; set; } = [];
    public List<string> LearningOutcomes { get; set; } = [];
    public int EngagementScore { get; set; }
}