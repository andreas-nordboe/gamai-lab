using GamAILab.Frontend.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Components.Pages.Core;

public partial class Register : ComponentBase
{
    [Inject] private IAuthenticationService AuthenticationService { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfrimPassword { get; set; }

    private async Task RegisterUserAsync()
    {
        try
        {
            if (Password != ConfrimPassword)
            {
                Snackbar.Add("Passwords do not match", Severity.Error);
                return;
            }
            
            var registerResponse = await AuthenticationService.RegisterAsync(Email, Password);
            
            Snackbar.Add("Registration Successful. Logging in.", Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Registration failed: {ex.Message}", Severity.Error);
        }
    }
    
}