namespace GamAILab.Shared.Models;

public class User
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; } // TODO might leave this out initially
    public string Type { get; set; }
}