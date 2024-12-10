using System.Globalization;
using Serilog;
using Serilog.Exceptions;

namespace Neoron.API.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring logging.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Adds custom logging configuration to the host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
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
                        .WriteTo.Console(
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                            formatProvider: CultureInfo.InvariantCulture)
                        .WriteTo.File(
                            "logs/myapp.txt", 
                            rollingInterval: RollingInterval.Day,
                            formatProvider: CultureInfo.InvariantCulture);
                });
        }
    }
}
