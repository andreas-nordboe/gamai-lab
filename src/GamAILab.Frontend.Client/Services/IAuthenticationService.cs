using GamAILab.Shared.Models.Authentication;

namespace GamAILab.Frontend.Client.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResponse?> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
}