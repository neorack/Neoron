using Xunit;

namespace Neoron.API.Tests.Fixtures;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

[CollectionDefinition("Integration")] 
public class IntegrationCollection : ICollectionFixture<TestWebApplicationFactory<Program>>
{
}

public class TestCollectionFixture : IAsyncLifetime
{
    public TestWebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }
    public ApplicationDbContext DbContext { get; }

    public TestCollectionFixture()
    {
        Factory = new TestWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        DbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await Factory.DisposeAsync();
        Client.Dispose();
    }
}

[CollectionDefinition("TestApi")]
public class ApiTestCollection : ICollectionFixture<TestCollectionFixture>
{
}
