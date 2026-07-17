using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Components;

public partial class RedirectToLogin : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        NavigationManager.NavigateTo("/login");
    }
}