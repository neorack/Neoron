using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neoron.AppHost;
using Neoron.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add the API service
builder.Services.AddHostedService<ApiHostedService>();

// Configure API project
var apiBuilder = builder.AddProject<Projects.Neoron_API>("api");
apiBuilder.WithReference(builder.AddProject<Projects.Neoron_ServiceDefaults>("servicedefaults"));

var app = builder.Build();

// Map default health check and metrics endpoints
app.MapDefaultEndpoints();

await app.RunAsync();
