using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SendHub.Web.Components.Layout;

public partial class MainLayout
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool _isDarkMode;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var stored = await JS.InvokeAsync<string?>("localStorage.getItem", "sendhub-darkmode");
            _isDarkMode = stored == "true";
            StateHasChanged();
        }
    }

    private async Task ToggleDarkMode()
    {
        _isDarkMode = !_isDarkMode;
        await JS.InvokeVoidAsync("localStorage.setItem", "sendhub-darkmode", _isDarkMode.ToString().ToLowerInvariant());
    }
}
