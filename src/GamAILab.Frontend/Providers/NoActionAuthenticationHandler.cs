using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GamAILab.Frontend.Providers;

public class NoActionAuthenticationHandler :  AuthenticationHandler<AuthenticationSchemeOptions>
{
    public NoActionAuthenticationHandler(UrlEncoder options, IOptionsMonitor<AuthenticationSchemeOptions> logger, ILoggerFactory encoder) 
        : base(logger, encoder, options)
    {
        
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}