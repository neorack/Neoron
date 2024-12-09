using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Neoron.API.Tests.TestConstants;

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
            new Claim(ClaimTypes.Name, _configuration["TestAuthUserName"] ?? Auth.TestUserName),
            new Claim(ClaimTypes.NameIdentifier, _configuration["TestAuthUserId"] ?? Auth.TestUserId),
            new Claim(ClaimTypes.Role, _configuration["TestAuthUserRole"] ?? Auth.TestUserRole)
        };
        var identity = new ClaimsIdentity(claims, _configuration["TestAuthScheme"] ?? Auth.TestAuthScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, _configuration["TestAuthScheme"] ?? Auth.TestAuthScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public const string AuthenticationScheme = "Test";
}

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
