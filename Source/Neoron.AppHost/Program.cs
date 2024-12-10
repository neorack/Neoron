using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neoron.AppHost;
using Neoron.ServiceDefaults;

namespace Neoron.AppHost;

/// <summary>
/// The main entry point for the application host.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults from ServiceDefaults project
        builder.AddServiceDefaults();

        // Add hosted service
        builder.Services.AddHostedService<ApiHostedService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        await app.RunAsync().ConfigureAwait(false);
    }
}
