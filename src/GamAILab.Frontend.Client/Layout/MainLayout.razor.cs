using GamAILab.Frontend.Client.Dialogs;
using GamAILab.Frontend.Client.Providers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace GamAILab.Frontend.Client.Layout;

public partial class MainLayout
{
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private JWTAuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    private int _activeTabIndex;
    private bool _isDarkMode = true;
    private bool IsUnreal => NavigationManager.Uri.Contains("unreal=true", StringComparison.OrdinalIgnoreCase);

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += LocationChanged;
        if (IsUnreal)
        {
            _isDarkMode = false;
        }
    }

    private readonly MudTheme _theme = new()
    {
        // TODO add dark-mode later
        PaletteLight = new PaletteLight()
        {
            Primary = "#52c49c", // 435291 more purple-ish
            Secondary = "#40ffbc",
            Background = "#ffff",
            TextDisabled = "#ffffff"
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#52c49c", // 435291 more purple-ish
            Secondary = "#40ffbc",
            Background = "#1e243b",
            TextDisabled = "#ffffff"
        }
    };

    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
    
    private void NavigateTo(string url)
    {
        NavigationManager.NavigateTo(url);
    }
    
    private async Task LogoutUserAsync()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth =  true, MaxWidth = MaxWidth.ExtraSmall };
        
        var dialog = await DialogService.ShowAsync<LogoutDialog>("Logout", options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            await AuthenticationStateProvider.SetUserLoggedOut();
            NavigationManager.NavigateTo("/login", forceLoad: true);
        }
    }
}