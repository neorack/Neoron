using Microsoft.Extensions.Hosting;

namespace Neoron.AppHost;

/// <summary>
/// A background service that hosts the API application.
/// </summary>
internal sealed class ApiHostedService : BackgroundService
{
    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that can be used to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
