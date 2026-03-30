// <copyright file="RedisConfigurationHelper.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Globalization;
using System.Text;

namespace EleveRats.Core.Infra.Caching;

/// <summary>
/// Helper class for Redis-related configuration and connection string normalization.
/// </summary>
internal static class RedisConfigurationHelper
{
    /// <summary>
    /// Normalizes a Redis connection string.
    /// Supports both StackExchange format and redis:// or rediss:// URIs.
    /// </summary>
    /// <param name="connectionString">The raw connection string or URI.</param>
    /// <returns>A normalized connection string compatible with StackExchange.Redis.</returns>
    public static string GetConnectionString(string connectionString)
    {
        if (
            string.IsNullOrWhiteSpace(connectionString)
            || !Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri)
            || (
                !uri.Scheme.Equals("redis", StringComparison.OrdinalIgnoreCase)
                && !uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return connectionString;
        }

        var options = new StringBuilder();
        options.Append(CultureInfo.InvariantCulture, $"{uri.Host}:{uri.Port}");

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            string[] userInfo = uri.UserInfo.Split(':');

            // Check for password (usually user:password or just :password)
            string password = userInfo.Length > 1 ? userInfo[1] : userInfo[0];
            options.Append(CultureInfo.InvariantCulture, $",password={password}");
        }

        if (uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase))
        {
            options.Append(",ssl=true");
        }

        options.Append(",abortConnect=false");

        return options.ToString();
    }
}
