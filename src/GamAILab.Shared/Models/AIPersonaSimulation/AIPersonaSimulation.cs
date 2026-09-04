using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.AIPersonaSimulation;

// Stored in database for analytics
public class AIPersonaSimulation
{
    [Key] public int Id { get; set; }
    public List<AIPersona> Personas { get; set; }
    public string LLMModelUsed { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long GenerationTimeInMilliseconds { get; set; } 
}