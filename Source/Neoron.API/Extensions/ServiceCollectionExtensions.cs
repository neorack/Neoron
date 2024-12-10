using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Repositories;

namespace Neoron.API.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection to add application services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Discord-related services to the IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The updated IServiceCollection.</returns>
        public static IServiceCollection AddDiscordServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IDiscordMessageRepository, DiscordMessageRepository>();

            return services;
        }
    }
}
