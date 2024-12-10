using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Neoron.ServiceDefaults;

/// <summary>
/// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
/// This project should be referenced by each service project in your solution.
/// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults.
/// </summary>
public static class ServiceDefaultsExtensions
{
    private static readonly string[] DefaultAllowedSchemes = new[] { "https" };
    private static readonly string[] LiveHealthCheckTags = new[] { "live" };

    /// <summary>
    /// Adds service defaults to the specified builder.
    /// </summary>
    /// <param name="builder">The builder to add service defaults to.</param>
    /// <returns>The builder with service defaults added.</returns>
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        // Configure HTTP client defaults
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry for the specified builder.
    /// </summary>
    /// <param name="builder">The builder to configure OpenTelemetry for.</param>
    /// <returns>The builder with OpenTelemetry configured.</returns>
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Adds default health checks to the specified builder.
    /// </summary>
    /// <param name="builder">The builder to add default health checks to.</param>
    /// <returns>The builder with default health checks added.</returns>
    public static WebApplicationBuilder AddDefaultHealthChecks(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), LiveHealthCheckTags);

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
        // See https://aka.ms/dotnet/aspire/healthchecks for details.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready
            app.MapHealthChecks("/health");

            // Only health checks tagged with "live" must pass
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
            });
        }

        return app;
    }

    /// <summary>
    /// Adds OpenTelemetry exporters to the specified builder.
    /// </summary>
    /// <param name="builder">The builder to add OpenTelemetry exporters to.</param>
    /// <returns>The builder with OpenTelemetry exporters added.</returns>
    private static WebApplicationBuilder AddOpenTelemetryExporters(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.ConfigureOpenTelemetryMeterProvider(options => options.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(options => options.AddOtlpExporter());
        }

        return builder;
    }
}
