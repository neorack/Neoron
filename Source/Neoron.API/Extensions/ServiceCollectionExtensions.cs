using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Repositories;

namespace Neoron.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IDiscordMessageRepository, DiscordMessageRepository>();

            return services;
        }
    }
}
