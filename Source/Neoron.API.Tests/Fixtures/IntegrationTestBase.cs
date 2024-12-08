using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Data;
using System.Net.Http.Headers;
using Xunit;

namespace Neoron.API.Tests.Fixtures;

public class IntegrationTestBase : IClassFixture<TestWebApplicationFactory<Program>>
{
    protected readonly TestWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;

    protected IntegrationTestBase(TestWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected virtual void Cleanup()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
    }
}
