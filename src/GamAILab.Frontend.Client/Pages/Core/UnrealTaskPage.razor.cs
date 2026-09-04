
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class UnrealTaskPage : ComponentBase
{
    [Parameter]
    public int TaskId { get; set; }

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var jwtToken = await JsRuntime.InvokeAsync<string?>("eval", "new URLSearchParams(window.location.hash.substring(1)).get('token')");

        if (string.IsNullOrWhiteSpace(jwtToken))
            return;

        await LocalStorage.SetItemAsync("authToken", jwtToken);

        NavigationManager.NavigateTo($"/code-task/{TaskId}?unreal=true", forceLoad: true);
    }
}