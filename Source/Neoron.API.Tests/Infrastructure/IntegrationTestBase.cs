using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Data;
using Neoron.API.Tests.Fixtures;
using System.Net.Http.Json;
using Xunit;

namespace Neoron.API.Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;

    protected IntegrationTestBase()
    {
        Factory = new TestWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        DbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        Client.Dispose();
    }

    protected async Task<T> SendAsync<T>(HttpRequestMessage request)
    {
        var response = await Client.SendAsync(request);
        response.Should().BeSuccessful();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
