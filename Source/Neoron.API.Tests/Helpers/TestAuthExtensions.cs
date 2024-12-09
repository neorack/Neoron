using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Neoron.API.Tests.Helpers;

public static class TestAuthExtensions
{
    public static IServiceCollection AddTestAuth(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
            options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
        })
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
            TestAuthHandler.AuthenticationScheme, 
            options => { });

        return services;
    }
}
