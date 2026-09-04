namespace GamAILab.Shared.Models.Authentication;

public class AuthenticationUserResponse
{
    public string Id { get; set; }
    public string? Email { get; set; }
    public IList<string> Roles { get; set; }  
}