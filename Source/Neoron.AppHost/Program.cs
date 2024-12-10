using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Neoron_API>("api");

await builder.Build().RunAsync();
