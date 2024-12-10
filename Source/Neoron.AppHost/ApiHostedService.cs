using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Neoron.AppHost;

/// <summary>
/// A background service that hosts the API application.
/// </summary>
internal sealed class ApiHostedService : BackgroundService
{
    private static readonly Action<ILogger, Exception?> RunningLog = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, "Running"),
        "ApiHostedService is running.");

    private static readonly Action<ILogger, Exception?> StoppingLog = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, "Stopping"),
        "ApiHostedService is stopping.");

    private readonly ILogger<ApiHostedService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiHostedService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ApiHostedService(ILogger<ApiHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger;
    }

    /// <summary>
    /// Handles stopping the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        StoppingLog(logger, null);
        await base.StopAsync(stoppingToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that can be used to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RunningLog(logger, null);
        return Task.CompletedTask;
    }
}
