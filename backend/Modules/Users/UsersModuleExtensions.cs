// <copyright file="UsersModuleExtensions.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

using EleveRats.Core.Application.Interfaces;
using EleveRats.Core.Infra.Persistence;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Application.Services;
using EleveRats.Modules.Users.Infra.Persistence;
using EleveRats.Modules.Users.Infra.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EleveRats.Modules.Users;

/// <summary>
/// Extension methods for registering the Users module dependencies.
/// </summary>
internal static class UsersModuleExtensions
{
    /// <summary>
    /// Registers all infrastructure and application services for the Users module.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Fetch the prioritized connection string (checks DATABASE_URL first)
        string? connectionString = DbConnectionStringHelper.GetConnectionString(configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The database connection string is missing or empty. Please check your environment variables or appsettings."
            );
        }

        // 2. Register the DbContext with PostgreSQL
        services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));

        // 3. Register Module Cache Options
        services.Configure<UsersCacheOptions>(configuration.GetSection("Cache:Modules:Users"));

        // 4. Register Repositories (Decorated with Cache)
        services.AddScoped<UserRepository>();
        services.AddScoped<IUserRepository>(sp => new CachedUserRepository(
            sp.GetRequiredService<UserRepository>(),
            sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<IOptions<UsersCacheOptions>>()
        ));

        services.AddScoped<ProfileRepository>();
        services.AddScoped<IProfileRepository>(sp => new CachedProfileRepository(
            sp.GetRequiredService<ProfileRepository>(),
            sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<IOptions<UsersCacheOptions>>()
        ));

        services.AddScoped<OrganizationRepository>();
        services.AddScoped<IOrganizationRepository>(sp => new CachedOrganizationRepository(
            sp.GetRequiredService<OrganizationRepository>(),
            sp.GetRequiredService<ICacheService>(),
            sp.GetRequiredService<IOptions<UsersCacheOptions>>()
        ));

        // 5. Register RefreshToken Repository (no cache needed for token operations)
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // 6. Register Unit of Work (UsersDbContext implements IUsersUnitOfWork)
        services.AddScoped<IUsersUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

        // 7. Register Application Services
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
