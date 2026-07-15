using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Components.Layout;

public partial class MainLayout
{
    [Inject] private NavigationManager NavigationManager { get; set; }
    private int _activeTabIndex;
    
    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#3a446e"
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#3a446e"
        }
    };

    private void NavigateTo(string url)
    {
        NavigationManager.NavigateTo(url);
    }
    
    private Task LogoutUserAsync()
    {
        throw new NotImplementedException();
    }

}