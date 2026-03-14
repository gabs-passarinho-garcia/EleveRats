// <copyright file="UsersDbContextFactory.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EleveRats.Modules.Users.Infra.Persistence;

/// <summary>
/// Factory for creating the UsersDbContext at design-time.
/// Used exclusively by the EF Core CLI tools (e.g., for creating migrations).
/// </summary>
internal sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        // 1. Reads the appsettings.json from the root of the backend folder
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // 2. Extracts the connection string
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fallback for local development if the connection string is missing in appsettings
            connectionString = "Host=localhost;Database=eleverats_dev;Username=postgres;Password=postgres";
        }

        // 3. Builds the DbContextOptions
        var builder = new DbContextOptionsBuilder<UsersDbContext>();
        builder.UseNpgsql(connectionString);

        return new UsersDbContext(builder.Options);
    }
}
