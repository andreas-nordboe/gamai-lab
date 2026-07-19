using GamAILab.Frontend.Client.Providers;
using GamAILab.Frontend.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class Register : ComponentBase
{
    [Inject] private IAuthenticationService AuthenticationService { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private JWTAuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    
    // Register fields
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfrimPassword { get; set; }
    
    // Password toggle
    bool showPassword;
    InputType PasswordInput = InputType.Password;
    string PasswordInputIcon = Icons.Material.Filled.Visibility;
    
    // Confirm password toggle
    bool showConfirmPassword;
    InputType ConfirmPasswordInput = InputType.Password;
    string ConfirmPasswordInputIcon = Icons.Material.Filled.Visibility;
    
    

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
            
            await AuthenticationStateProvider.SetUserLoggedIn(registerResponse.AccessToken);
            
            Snackbar.Add("Registration Successful. Logging in.", Severity.Success);
            NavigationManager.NavigateTo("/", replace: true);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Registration failed: {ex.Message}", Severity.Error);
        }
    }
    
    void TogglePassword()
    {
        if(showPassword)
        {
            showPassword = false;
            PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
            PasswordInput = InputType.Password;
        }
        else {
            showPassword = true;
            PasswordInputIcon = Icons.Material.Filled.Visibility;
            PasswordInput = InputType.Text;
        }
    }
    
    void ToggleConfirmPassword()
    {
        if(showConfirmPassword)
        {
            showConfirmPassword = false;
            ConfirmPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
            ConfirmPasswordInput = InputType.Password;
        }
        else {
            showConfirmPassword = true;
            ConfirmPasswordInputIcon = Icons.Material.Filled.Visibility;
            ConfirmPasswordInput = InputType.Text;
        }
    }
}