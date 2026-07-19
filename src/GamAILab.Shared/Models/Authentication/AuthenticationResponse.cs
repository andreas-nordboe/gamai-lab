namespace GamAILab.Shared.Models.Authentication;

public class AuthenticationResponse
{
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public AuthenticationUserResponse User { get; set; }
}