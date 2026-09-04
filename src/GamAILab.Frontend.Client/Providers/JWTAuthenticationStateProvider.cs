using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace GamAILab.Frontend.Client.Providers;

public class JWTAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private CancellationTokenSource? _jwtTokenExpirationTokenCancellationSource;

    public JWTAuthenticationStateProvider(ILocalStorageService localStorage, NavigationManager navigationManager)
    {
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

   

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var jwtToken = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (HasTokenExpired(jwtToken))
            {
                await SetUserLoggedOut();
                _navigationManager.NavigateTo("/login", forceLoad: true);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            
            StartTokenExpiryHandler(jwtToken);

            var claims = ParseClaims(jwtToken);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (Exception e)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private IEnumerable<Claim> ParseClaims(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return new List<Claim>();

        var handler = new JwtSecurityTokenHandler();
        
        var jwtToken = handler.ReadJwtToken(accessToken);
        return jwtToken.Claims;
    }

    private bool HasTokenExpired(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            if (jwtToken.ValidTo < DateTime.UtcNow)
                return true;

            return false;
        }
        catch (Exception e)
        {
            return true;
        }
    }

    public async Task SetUserLoggedIn(string accessToken)
    {
        await _localStorage.SetItemAsync("authToken", accessToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    
    public async Task SetUserLoggedOut()
    {
        _jwtTokenExpirationTokenCancellationSource?.Cancel();
        await _localStorage.RemoveItemAsync("authToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    
    // to kick client instead of showing awful failed requests 
    private void StartTokenExpiryHandler(string accessToken)
    {
        _jwtTokenExpirationTokenCancellationSource?.Cancel();
        _jwtTokenExpirationTokenCancellationSource = new CancellationTokenSource();
        
        var cancellationToken = _jwtTokenExpirationTokenCancellationSource.Token;
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);
        var timeUntilExpiry = jwtToken.ValidTo - DateTime.UtcNow;

        if (timeUntilExpiry < TimeSpan.Zero)
        {
            return;
        }
        
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeUntilExpiry, cancellationToken );
                await SetUserLoggedOut();
                _navigationManager.NavigateTo("/login");
            }
            catch (TaskCanceledException)
            {
                // TODO for later, handle user logout or when token is replaced, could potentially be mentioned in the report for security tampering in the STRIDE table..  
            }
        });
    }
    
}