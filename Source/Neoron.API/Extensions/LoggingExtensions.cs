using Serilog;
using Serilog.Events;

namespace Neoron.API.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddCustomLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File("logs/neoron-.log", 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName());
    }
}
