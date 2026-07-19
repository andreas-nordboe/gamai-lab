using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class Home : ComponentBase
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthState { get; set; }

    public string Username { get; set; } = "No User";

    protected override async Task OnInitializedAsync()
    {
        if (AuthState != null)
        {
            var auth = await AuthState;
            var user = auth.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                Username = user.Identity.Name ?? user.FindFirst(c => c.Type == "email")?.Value ??  "No User";
            }
        }
    }
}