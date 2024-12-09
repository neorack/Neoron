using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Neoron.API.Tests.Helpers;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, _configuration["TestAuthUserName"] ?? "Test User"),
            new Claim(ClaimTypes.NameIdentifier, _configuration["TestAuthUserId"] ?? "test-user-id"),
            new Claim(ClaimTypes.Role, _configuration["TestAuthUserRole"] ?? "admin")
        };
        var identity = new ClaimsIdentity(claims, _configuration["TestAuthScheme"] ?? "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, _configuration["TestAuthScheme"] ?? "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public static class AuthTestHelper
{
    public static void AddTestAuth(this IServiceCollection services)
    {
        services.AddAuthentication(defaultScheme: "Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
    }
}
