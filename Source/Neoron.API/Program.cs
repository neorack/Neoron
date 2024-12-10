[assembly: System.Runtime.CompilerServices.EntryPoint]

using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Use bootstrap logger during startup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up Neoron API");

    // Add enhanced Serilog configuration
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                formatProvider: CultureInfo.InvariantCulture);
    });

    Log.Information("Configuring application services...");

    builder.AddServiceDefaults();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc(
            name: "v1",
            info: new OpenApiInfo
            {
                Title = "Neoron API",
                Version = "v1",
            });
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Neoron API v1");
        });
        app.UseDeveloperExceptionPage();
    }

    // Add diagnostic middleware
    app.Use(async (context, next) =>
    {
        Log.Debug(
            messageTemplate: "Processing request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);
        await next().ConfigureAwait(false);
    });

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // Add test endpoints
    app.MapGet("/test", () =>
    {
        Log.Information("Test endpoint called");
        var diagnostics = new
        {
            message = "API is running",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName,
            isDevMode = app.Environment.IsDevelopment(),
            authEnabled = app.Configuration["AzureAd:ClientId"] != null,
        };
        return Results.Ok(diagnostics);
    });

    app.MapGet("/test/error", () =>
    {
        Log.Warning("Test error endpoint called");
        throw new InvalidOperationException("Test exception to verify error handling");
    });

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
