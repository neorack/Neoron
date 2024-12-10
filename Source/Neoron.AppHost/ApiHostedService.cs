using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Neoron.AppHost;

internal sealed class ApiHostedService : BackgroundService
{
    private static readonly Action<ILogger, Exception?> RunningLog = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, "Running"),
        "API service is running.");

    private static readonly Action<ILogger, Exception?> StoppingLog = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, "Stopping"),
        "API service is stopping.");

    private readonly ILogger<ApiHostedService> _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    public ApiHostedService(
        ILogger<ApiHostedService> logger,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        StoppingLog(_logger, null);
        await base.StopAsync(stoppingToken).ConfigureAwait(false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("ApiHostedService starting...");
            RunningLog(_logger, null);
            
            // Register application start with detailed error handling
            _appLifetime.ApplicationStarted.Register(() =>
            {
                try
                {
                    _logger.LogInformation("Application has started successfully");
                    _logger.LogInformation($"Current directory: {Environment.CurrentDirectory}");
                    _logger.LogInformation($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in application startup registration");
                }
            });

            // Add detailed cancellation token handling
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Cancellation requested for ApiHostedService");
            });

            _logger.LogInformation("ApiHostedService started successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error starting ApiHostedService");
            throw;
        }
    }
}
