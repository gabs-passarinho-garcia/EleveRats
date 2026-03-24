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

using System;
using EleveRats.Modules.Users.Infra.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Fetch the connection string from your appsettings.Development.json
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The 'DefaultConnection' string is missing or empty. Please check your appsettings.");
        }

        // 2. Register the DbContext with PostgreSQL
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Future plays: Here is where we will register the IUserRepository and other services
        // services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
