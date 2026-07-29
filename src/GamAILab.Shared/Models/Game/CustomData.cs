using System.ComponentModel.DataAnnotations;

namespace GamAILab.Shared.Models.Game;

public class CustomData
{
    [Key]
    public int Id { get; set; }
    public string? UserId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
}