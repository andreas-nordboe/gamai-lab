using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Components.Redirectors;

public partial class RedirectToLogin : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (!authState.User.Identity?.IsAuthenticated ?? true)
        {
            var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
        }
    }
}