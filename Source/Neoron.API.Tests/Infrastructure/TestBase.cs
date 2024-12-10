using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Data;
using Neoron.API.Tests.Fixtures;

namespace Neoron.API.Tests.Infrastructure;

public abstract class TestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;

    protected TestBase()
    {
        Factory = new TestWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        DbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await DbContext.DisposeAsync();
    }
}
