using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Core.Infra.Caching;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Infra.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace EleveRats.Tests.Core.Infra.Web;

public class EleveRatsApplicationFactory : WebApplicationFactory<Program>
{
    internal ICacheService MockCacheService { get; } = Substitute.For<ICacheService>();
    internal ITokenService MockTokenService { get; } = Substitute.For<ITokenService>();

    public EleveRatsApplicationFactory()
    {
        System.Environment.SetEnvironmentVariable(
            "DATABASE_URL",
            "Host=localhost;Database=dummy;Username=dummy;Password=dummy"
        );
        MockCacheService
            .GetAsync<string>(Arg.Any<string>())
            .Returns(Task.FromResult((string?)"active"));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (context, conf) =>
                conf.AddInMemoryCollection(
                    new System.Collections.Generic.Dictionary<string, string?>
                    {
                        {
                            "ConnectionStrings:DefaultConnection",
                            "Host=localhost;Database=dummy;Username=dummy;Password=dummy"
                        },
                        {
                            // Dummy 256-bit key to satisfy Program.cs startup — JWT Bearer is
                            // fully replaced by the Test scheme in ConfigureServices below.
                            "JwtSettings:SecretKey",
                            "test-only-secret-key-32-chars!!"
                        },
                    }
                )
        );

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            ServiceDescriptor? descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<UsersDbContext>)
            );
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register the DbContext using an In-Memory database
            services.AddDbContext<UsersDbContext>(options =>
                options.UseInMemoryDatabase("EleveRatsTestDb")
            );

            // Override ICacheService with a mock to avoid Redis issues in tests
            ServiceDescriptor? cacheDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(ICacheService)
            );
            if (cacheDescriptor != null)
            {
                services.Remove(cacheDescriptor);
            }
            services.AddSingleton(MockCacheService);

            // Override ITokenService with a mock to control JWT generation and revocation easily in tests
            ServiceDescriptor? tokenDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(ITokenService)
            );
            if (tokenDescriptor != null)
            {
                services.Remove(tokenDescriptor);
            }
            services.AddScoped(_ => MockTokenService);

            // Configure Test Authentication
            // Remove all existing IConfigureOptions<AuthenticationOptions> to ensure the
            // Test scheme truly becomes the default and overrides any prior AddAuthentication() calls.
            var authConfigDescriptors = services
                .Where(d => d.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>))
                .ToList();
            foreach (ServiceDescriptor? d in authConfigDescriptors)
            {
                services.Remove(d);
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });
    }
}

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // By default, if the request has an "Authorization" header, we consider it authenticated for testing.
        if (!Request.Headers.Authorization.Any())
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims =
        [
            new Claim(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
                System.Guid.CreateVersion7().ToString()
            ),
            new Claim("profileId", System.Guid.CreateVersion7().ToString()),
            new Claim("orgId", System.Guid.CreateVersion7().ToString()),
            new Claim(ClaimTypes.Email, "testuser@eleverats.com"),
            new Claim(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,
                System.Guid.CreateVersion7().ToString()
            ),
        ];

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
