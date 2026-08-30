namespace GamAILab.Shared.Models.DTOs;

public class GenerateCodeTaskRequest
{
    public string Description { get; set; }
    public string GameStory { get; set; }
    public CodeTaskDifficulty CodeTaskDifficulty { get; set; }
}