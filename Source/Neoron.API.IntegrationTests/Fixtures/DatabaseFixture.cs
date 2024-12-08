using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Testcontainers.MsSql;
using Xunit;

namespace Neoron.API.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;
    public ApplicationDbContext DbContext { get; private set; } = null!;

    public DatabaseFixture()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        DbContext = new ApplicationDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }
}
