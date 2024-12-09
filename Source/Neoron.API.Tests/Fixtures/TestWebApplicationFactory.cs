using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neoron.API.Data;
using Neoron.API.Tests.Helpers;
using Testcontainers.MsSql;

namespace Neoron.API.Tests.Fixtures;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncDisposable where TProgram : class
{
    private readonly MsSqlContainer _sqlContainer;
    private bool _disposed;

    public TestWebApplicationFactory()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithName($"sql_test_{Guid.NewGuid()}")
            .WithPassword("Strong_password_123!")
            .Build();
    }

    protected override async void ConfigureWebHost(IWebHostBuilder builder)
    {
        await _sqlContainer.StartAsync();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_sqlContainer.GetConnectionString());
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Add test authentication
            services.AddTestAuth();

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                db.Database.Migrate();
                
                // Seed test data if needed
                SeedTestData(db);
            }
        });
    }

    private static void SeedTestData(ApplicationDbContext db)
    {
        // Add test data seeding logic here
        db.SaveChanges();
    }

    public new async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_sqlContainer != null)
            {
                await _sqlContainer.DisposeAsync();
            }
            _disposed = true;
        }

        await base.DisposeAsync();
    }
}
