using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
    /// This project should be referenced by each service project in your solution.
    /// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults.
    /// </summary>
    public static class ServiceDefaultsExtensions
    {
        private static readonly string[] AllowedSchemes = { "https" };

        /// <summary>
        /// Adds service defaults to the specified builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to add service defaults to.</param>
        /// <returns>The builder with service defaults added.</returns>
        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            builder.ConfigureOpenTelemetry();
            builder.AddDefaultHealthChecks();
            builder.Services.AddServiceDiscovery();
            builder.Services.ConfigureHttpClientDefaults(http =>
            {

                // Turn on resilience by default
                _ = http.AddStandardResilienceHandler();
                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            // Uncomment the following to restrict the allowed schemes for service discovery.
            // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
            // {
            //     options.AllowedSchemes = AllowedSchemes;
            // });

            return builder;
        }

        /// <summary>
        /// Configures OpenTelemetry for the specified builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to configure OpenTelemetry for.</param>
        /// <returns>The builder with OpenTelemetry configured.</returns>
        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            _ = builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            _ = builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    _ = metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    _ = tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation()

                        // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                        //.AddGrpcClientInstrumentation()
                        .AddHttpClientInstrumentation();
                });

            _ = builder.AddOpenTelemetryExporters();

            return builder;
        }

        /// <summary>
        /// Adds default health checks to the specified builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to add default health checks to.</param>
        /// <returns>The builder with default health checks added.</returns>
        public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            _ = builder.Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "live" });

            return builder;
        }

        /// <summary>
        /// Maps default endpoints for the specified application.
        /// </summary>
        /// <param name="app">The application to map default endpoints for.</param>
        /// <returns>The application with default endpoints mapped.</returns>
        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            // Adding health checks endpoints to applications in non-development environments has security implications.
            // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
            if (app.Environment.IsDevelopment())
            {
                // All health checks must pass for app to be considered ready to accept traffic after starting
                _ = app.MapHealthChecks("/health");

                // Only health checks tagged with the "live" tag must pass for app to be considered alive
                _ = app.MapHealthChecks("/alive", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live"),
                });
            }

            return app;
        }

        /// <summary>
        /// Adds OpenTelemetry exporters to the specified builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="builder">The builder to add OpenTelemetry exporters to.</param>
        /// <returns>The builder with OpenTelemetry exporters added.</returns>
        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }


            // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
            // if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            // {
            //     builder.Services.AddOpenTelemetry()
            //        .UseAzureMonitor();
            // }

            return builder;
        }
    }
}
