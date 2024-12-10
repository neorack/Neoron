using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neoron.AppHost;
using Neoron.ServiceDefaults;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure detailed logging first
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

    var logger = builder.Services.BuildServiceProvider()
        .GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Starting application initialization...");

    // Add service defaults
    builder.AddServiceDefaults();
    logger.LogInformation("Service defaults added");

    // Add the API service
    builder.Services.AddHostedService<ApiHostedService>();
    logger.LogInformation("API hosted service registered");

    // Configure API project
    var apiBuilder = builder.AddProject<Projects.Neoron_API>("api");
    apiBuilder.WithReference(builder.AddProject<Projects.Neoron_ServiceDefaults>("servicedefaults"));
    logger.LogInformation("API project configured");

    logger.LogInformation("Building application...");
    var app = builder.Build();

    // Map default health check and metrics endpoints
    app.MapDefaultEndpoints();
    logger.LogInformation("Default endpoints mapped");

    logger.LogInformation("Starting application...");
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
