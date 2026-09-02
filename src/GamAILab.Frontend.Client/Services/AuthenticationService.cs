using System.Net.Http.Json;
using GamAILab.Frontend.Client.Providers;
using GamAILab.Shared.Models.Authentication;

namespace GamAILab.Frontend.Client.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly JWTAuthenticationStateProvider _jwtAuthenticationStateProvider;

    public AuthenticationService(HttpClient httpClient, JWTAuthenticationStateProvider jwtAuthenticationStateProvider)
    {
        _httpClient = httpClient;
        _jwtAuthenticationStateProvider = jwtAuthenticationStateProvider;
    }
    

    public async Task<AuthenticationResponse?> LoginAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest()
        {
            Email = email,
            Password = password
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Invalid or empty password");
            
            throw new HttpRequestException("An internal error occured");
        }
        
        var loginResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken: cancellationToken);
        
        if (loginResponse?.AccessToken == null)
            throw new InvalidOperationException("Invalid or empty access token");
        
        await _jwtAuthenticationStateProvider.SetUserLoggedIn(loginResponse.AccessToken);
        
        return loginResponse;
    }
    
    public async Task<AuthenticationResponse?> RegisterAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var request = new RegisterRequest()
        {
            Email = email,
            Password = password
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);
    
        if (!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception("Registration failed, please try again.");
            
            throw new HttpRequestException("An internal error occured during registration");
        }
        
        var loginResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken: cancellationToken);
        
        return loginResponse;
    }
}