// <copyright file="BitwardenConfigurationExtensions.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace EleveRats.Core.Infra.Configuration;

/// <summary>
/// Extension methods for integrating the Bitwarden Secrets Manager into the
/// .NET <see cref="IConfigurationBuilder"/> pipeline.
/// </summary>
public static class BitwardenConfigurationExtensions
{
    /// <summary>The environment variable holding the Bitwarden Machine Account Access Token.</summary>
    private const string _accessTokenEnvVar = "BITWARDEN_ACCESS_TOKEN";

    /// <summary>The environment variable holding the Bitwarden Organization ID (GUID).</summary>
    private const string _organizationIdEnvVar = "BITWARDEN_ORGANIZATION_ID";

    /// <summary>The environment variable holding the Bitwarden Project ID (GUID) used to scope secret fetching.</summary>
    private const string _projectIdEnvVar = "BITWARDEN_PROJECT_ID";

    /// <summary>
    /// Adds the Bitwarden Secrets Manager as a configuration source, loading secrets into the
    /// standard .NET <see cref="IConfiguration"/> pipeline at application startup.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs a <strong>safe no-op</strong> if any of the required environment variables
    /// (<c>BITWARDEN_ACCESS_TOKEN</c>, <c>BITWARDEN_ORGANIZATION_ID</c>, <c>BITWARDEN_PROJECT_ID</c>)
    /// are absent. This ensures integration tests and local development environments without Bitwarden
    /// access remain unaffected.
    /// </para>
    /// <para>
    /// The source is added <strong>last</strong> in the configuration pipeline, allowing Bitwarden
    /// secrets to override values from <c>appsettings.json</c> and environment variables.
    /// This follows the standard .NET configuration precedence pattern (later sources win).
    /// </para>
    /// <para>
    /// <strong>Required Environment Variables:</strong>
    /// <list type="bullet">
    ///   <item><c>BITWARDEN_ACCESS_TOKEN</c> — Machine Account Access Token.</item>
    ///   <item><c>BITWARDEN_ORGANIZATION_ID</c> — Bitwarden Organization GUID.</item>
    ///   <item><c>BITWARDEN_PROJECT_ID</c> — Bitwarden Project GUID for secret scoping.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to configure.</param>
    /// <returns>The same <see cref="IConfigurationBuilder"/> instance for fluent chaining.</returns>
    public static IConfigurationBuilder AddBitwardenSecrets(this IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        string? accessToken = Environment.GetEnvironmentVariable(_accessTokenEnvVar);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Debug.WriteLine(
                $"[Bitwarden] {_accessTokenEnvVar} is not set. "
                    + "Bitwarden Secrets Manager will be skipped (safe for dev/test environments)."
            );
            return builder;
        }

        if (!TryGetGuidFromEnv(_organizationIdEnvVar, out Guid organizationId))
        {
            Debug.WriteLine(
                $"[Bitwarden] {_organizationIdEnvVar} is missing or invalid. "
                    + "Bitwarden Secrets Manager will be skipped."
            );
            return builder;
        }

        if (!TryGetGuidFromEnv(_projectIdEnvVar, out Guid projectId))
        {
            Debug.WriteLine(
                $"[Bitwarden] {_projectIdEnvVar} is missing or invalid. "
                    + "Bitwarden Secrets Manager will be skipped."
            );
            return builder;
        }

        BitwardenConfigurationOptions options = new()
        {
            AccessToken = accessToken,
            OrganizationId = organizationId,
            ProjectId = projectId,
        };

        return builder.Add(new BitwardenConfigurationSource(options));
    }

    private static bool TryGetGuidFromEnv(string variableName, out Guid value)
    {
        string? rawValue = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            value = Guid.Empty;
            return false;
        }

        return Guid.TryParse(rawValue, out value);
    }
}
