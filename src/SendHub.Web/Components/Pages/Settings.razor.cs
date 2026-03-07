using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SendHub.Web.Components.Pages;

public partial class Settings
{
    [Inject] private IApplicationSettings ApplicationSettings { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private string _watchFolder = string.Empty;
    private string _destinationFolder = string.Empty;
    private int _pollingIntervalSeconds;
    private string _smtpHost = string.Empty;
    private int _smtpPort;
    private string _smtpUsername = string.Empty;
    private string _smtpPassword = string.Empty;
    private string _smtpFrom = string.Empty;
    private string _smtpTo = string.Empty;
    private bool _smtpEnableSsl;

    protected override Task OnInitializedAsync()
    {
        _watchFolder = ApplicationSettings.WatchFolder;
        _destinationFolder = ApplicationSettings.DestinationFolder;
        _pollingIntervalSeconds = ApplicationSettings.PollingIntervalSeconds;
        _smtpHost = ApplicationSettings.SmtpHost;
        _smtpPort = ApplicationSettings.SmtpPort;
        _smtpUsername = ApplicationSettings.SmtpUsername ?? string.Empty;
        _smtpPassword = ApplicationSettings.SmtpPassword ?? string.Empty;
        _smtpFrom = ApplicationSettings.SmtpFrom;
        _smtpTo = ApplicationSettings.SmtpTo;
        _smtpEnableSsl = ApplicationSettings.SmtpEnableSsl;
        return Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        try
        {
            await ApplicationSettings.UpdateAsync("SendHub:WatchFolder", _watchFolder);
            await ApplicationSettings.UpdateAsync("SendHub:DestinationFolder", _destinationFolder);
            await ApplicationSettings.UpdateAsync("SendHub:PollingIntervalSeconds", _pollingIntervalSeconds.ToString());
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:Host", _smtpHost);
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:Port", _smtpPort.ToString());
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:Username", _smtpUsername);
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:Password", _smtpPassword);
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:From", _smtpFrom);
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:To", _smtpTo);
            await ApplicationSettings.UpdateAsync("SendHub:Email:Smtp:EnableSsl", _smtpEnableSsl.ToString().ToLowerInvariant());

            Snackbar.Add("Settings saved successfully.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to save settings: {ex.Message}", Severity.Error);
        }
    }
}
