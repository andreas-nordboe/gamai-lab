using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models;

public class CodeTask
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string DefaultCode { get; set; }
    public List<string> Examples { get; set; } // TODO string list works for now but I want to extend this into a class later for more flexibility
    public List<string> Constraints { get; set; } // TODO possibly make this a list of objects/classes that have constraint types
    public int Version { get; set; } = 1; // TODO make this increment every time task is updated
    public CodeTaskDifficulty Difficulty { get; set; }
    public int CurrencyReward { get; set; } = 10; // Default reward of 10 in case I forget to set it while creating the task
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}