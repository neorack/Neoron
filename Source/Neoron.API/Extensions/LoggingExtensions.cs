using System.Globalization;
using Serilog;
using Serilog.Exceptions;

namespace Neoron.API.Extensions
{
    public static class LoggingExtensions
    {
        public static IHostBuilder AddCustomLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                        .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day);
                });
        }
    }
}
