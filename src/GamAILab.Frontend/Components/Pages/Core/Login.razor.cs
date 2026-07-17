using GamAILab.Frontend.Providers;
using GamAILab.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace GamAILab.Frontend.Components.Pages.Core;

public partial class Login : ComponentBase
{
    [Inject] private IAuthenticationService AuthenticationService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private JWTAuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    
    public string Email { get; set; }
    public string Password { get; set; }

    private async Task LogInUserAsync()
    {
        try
        {
            var loginResponse = await AuthenticationService.LoginAsync(Email, Password);

            if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.AccessToken))
            {
                Snackbar.Add("Login Failed", Severity.Error);
                return;
            }
            
            string accessToken = loginResponse.AccessToken;
            await AuthenticationStateProvider.SetUserLoggedIn(accessToken);
            NavigationManager.NavigateTo("/");
        }
        catch (UnauthorizedAccessException)
        {
            Snackbar.Add("Invalid email or password", Severity.Error);
        }
        catch (HttpRequestException)
        {
            Snackbar.Add("An internal error occured", Severity.Error);
        }
    }
}