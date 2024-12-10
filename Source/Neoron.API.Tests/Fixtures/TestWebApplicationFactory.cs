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
    private bool _databaseInitialized;
    private IServiceProvider? _serviceProvider;

    public TestWebApplicationFactory()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithName($"sql_test_{Guid.NewGuid()}")
            .WithPassword("Strong_password_123!")
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .Build();
    }

    public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Services not initialized");

    protected override async void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (!_databaseInitialized)
        {
            await _sqlContainer.StartAsync();
            _databaseInitialized = true;
        }

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

            // Add mock services
            MockServices.AddMockServices(services);

            // Add test authentication
            services.AddTestAuth();

            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            db.Database.Migrate();
            
            // Initialize test data
            InitializeTestData(db).GetAwaiter().GetResult();
        });
    }

    private static async Task InitializeTestData(ApplicationDbContext db)
    {
        // Clear existing data
        db.Messages.RemoveRange(db.Messages);
        await db.SaveChangesAsync();

        // Add base test data
        var baseMessage = new DiscordMessage
        {
            MessageId = 1,
            ChannelId = 1,
            GuildId = 1,
            AuthorId = 1,
            Content = "Base test message",
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.Messages.AddAsync(baseMessage);
        await db.SaveChangesAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_serviceProvider is IDisposable disposableServices)
            {
                disposableServices.Dispose();
            }

            if (_sqlContainer != null)
            {
                await _sqlContainer.DisposeAsync().ConfigureAwait(false);
            }

            _disposed = true;
        }

        await base.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            if (_serviceProvider is IDisposable disposableServices)
            {
                disposableServices.Dispose();
            }
        }
        base.Dispose(disposing);
    }
}
