using Aspire.Hosting;

namespace Neoron.AppHost;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        var api = builder.AddProject<Projects.Neoron_API>("api");
        await builder.Build().RunAsync();
        return 0;
    }
}
