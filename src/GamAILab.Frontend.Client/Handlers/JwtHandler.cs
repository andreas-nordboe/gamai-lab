using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace GamAILab.Frontend.Client.Handlers;

public sealed class JwtHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public JwtHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var jwtToken = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(jwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}