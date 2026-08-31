using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GamAILab.Frontend.Client.Layout;

public partial class UnrealLayout
{
    // Same as MainLayout but it's handy to be able to change these for Unreal specifically later
    private bool _isDarkMode = true;
    private readonly MudTheme _theme = new()
    {
        // TODO add dark-mode later
        PaletteLight = new PaletteLight()
        {
            Primary = "#435291",
            //Background = "#1e243b"
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#52c49c", // 435291 more purple-ish
            Secondary = "#40ffbc",
            Background = "#1e243b",
            TextDisabled = "#ffffff"
        }
    };
}