// <copyright file="DbConnectionStringHelper.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using Microsoft.Extensions.Configuration;

namespace EleveRats.Core.Infra.Persistence;

/// <summary>
/// Helper to extract and format the database connection string from various configuration sources.
/// </summary>
internal static class DbConnectionStringHelper
{
    /// <summary>
    /// Gets the prioritized connection string.
    /// Priority order:
    /// 1. DATABASE_URL (environment variable or config root) - handles URI format.
    /// 2. ConnectionStrings:DefaultConnection (from appsettings or env).
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A standard ADO.NET PostgreSQL connection string.</returns>
    public static string? GetConnectionString(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Try to get DATABASE_URL (usually from .env or system env)
        string? databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ParseDatabaseUrl(databaseUrl);
        }

        // 2. Fallback to the standard ConnectionStrings:DefaultConnection
        return configuration.GetConnectionString("DefaultConnection");
    }

    /// <summary>
    /// Parses a database URL (like postgresql://user:pass@host:port/db) into a standard connection string.
    /// If it's already in the KV format, returns it as is.
    /// </summary>
    private static string ParseDatabaseUrl(string databaseUrl)
    {
        if (
            !databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        )
        {
            return databaseUrl; // Probably already in Host=... format
        }

        try
        {
            var uri = new Uri(databaseUrl);
            string[] userInfo = uri.UserInfo.Split(':');
            string username = userInfo[0];
            string password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            string host = uri.Host;
            int port = uri.Port > 0 ? uri.Port : 5432;
            string database = uri.LocalPath.TrimStart('/');

            // Defaulting to SSL Require for external DBs like Neon
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            // If parsing fails for any reason, return the raw string and let Npgsql try to handle it or error out
            Console.WriteLine($"[WARNING] Failed to parse DATABASE_URL: {ex.Message}");
            return databaseUrl;
        }
    }
}
