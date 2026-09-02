using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using GamAILab.Frontend.Client.Providers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace GamAILab.Frontend.Client.Handlers;

public sealed class JwtHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly JWTAuthenticationStateProvider _authStateProvider;

    public JwtHandler(ILocalStorageService localStorage, JWTAuthenticationStateProvider authStateProvider, NavigationManager navigationManager)
    {
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var jwtToken = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(jwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Logout user once requests start returning 401 unauthorised, there could potentially also be a warning before logout using Snackbar
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          
            await _authStateProvider.SetUserLoggedOut();
            _navigationManager.NavigateTo("/login", forceLoad: true);
        }
        
        return response;
    }
    
    
}