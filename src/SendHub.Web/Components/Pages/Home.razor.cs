using Microsoft.AspNetCore.Components;

namespace SendHub.Web.Components.Pages;

public partial class Home
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        Navigation.NavigateTo("/settings");
    }
}
