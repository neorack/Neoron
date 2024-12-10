using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neoron.AppHost;
using Neoron.ServiceDefaults;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure logging first
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

    // Add service defaults before other services
    builder.AddServiceDefaults();
    
    // Add services to the container
    builder.Services.AddHostedService<ApiHostedService>();

    // Add service discovery
    builder.Services.AddServiceDiscovery();

    // Add health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Map health checks and default endpoints
    app.MapDefaultEndpoints();

    app.Logger.LogInformation("Application built and configured successfully");
    app.Logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
    app.Logger.LogInformation($"Application Name: {builder.Environment.ApplicationName}");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup failed: {ex}");
    // Add environment information
    Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
    Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
    throw;
}
