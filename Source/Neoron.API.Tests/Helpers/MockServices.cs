using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;

namespace Neoron.API.Tests.Helpers;

public static class MockServices
{
    public static void AddMockServices(IServiceCollection services)
    {
        services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                "Test", options => { });
    }
}
