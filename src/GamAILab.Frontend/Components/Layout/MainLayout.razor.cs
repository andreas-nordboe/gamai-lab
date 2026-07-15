using GamAILab.Frontend.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Components.Layout;

public partial class MainLayout
{
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    private int _activeTabIndex;
    private bool _isDarkMode = true;
    
    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#3a446e",
            //Background = "#1e243b"
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#3a446e",
            Background = "#1e243b"
        }
    };

    private void NavigateTo(string url)
    {
        NavigationManager.NavigateTo(url);
    }
    
    private async Task LogoutUserAsync()
    {
        Console.WriteLine("Logging out...");
        var options = new DialogOptions { CloseOnEscapeKey = true };
        
        var dialog = await DialogService.ShowAsync<LogoutDialog>("Logout", options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            // TODO Logout
        }
    }

}