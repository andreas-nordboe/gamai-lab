using GamAILab.Frontend.Client.Providers;
using GamAILab.Frontend.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class Login : ComponentBase
{
    [Inject] private IAuthenticationService AuthenticationService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private JWTAuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    
    // Password toggle
    bool showPassword;
    InputType PasswordInput = InputType.Password;
    string PasswordInputIcon = Icons.Material.Filled.Visibility;
    
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
    
    void TogglePassword()
    {
        showPassword = !showPassword;
        
        PasswordInput = showPassword ? InputType.Text : InputType.Password;
        PasswordInputIcon = showPassword ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility;
    }
}