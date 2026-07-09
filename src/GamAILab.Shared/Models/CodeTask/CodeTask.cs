namespace GamAILab.Shared.Models;

public class CodeTask
{
    public int CodeTaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string DefaultCode { get; set; }
    public List<string> Examples { get; set; } // TODO string list works for now but I want to extend this into a class later for more flexibility
    public List<string> Constraints { get; set; } // TODO possibly make this a list of objects/classes that have constraint types
    public CodeTaskDifficulty Difficulty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}