using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Neoron_API>("neoron-api");

builder.Build().Run();
