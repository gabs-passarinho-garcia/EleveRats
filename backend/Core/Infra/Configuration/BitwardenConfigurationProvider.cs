// <copyright file="BitwardenConfigurationProvider.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// A custom .NET configuration provider that loads application secrets from the
/// Bitwarden Secrets Manager (BSM) during application startup.
/// </summary>
/// <remarks>
/// This provider authenticates with BSM using a Machine Account Access Token and fetches
/// secrets scoped to a specific Bitwarden project. Secrets are injected into the standard
/// <see cref="IConfiguration"/> pipeline, making them available via <c>IOptions&lt;T&gt;</c>
/// and any other configuration-bound abstraction without exposing values in source code or
/// committed config files.
/// </remarks>
internal sealed class BitwardenConfigurationProvider : ConfigurationProvider
{
    private readonly BitwardenConfigurationOptions _options;
    private readonly IBitwardenClientWrapper _clientWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitwardenConfigurationProvider"/> class.
    /// This constructor is used by the configuration source in production.
    /// </summary>
    /// <param name="options">The Bitwarden authentication and filtering options.</param>
    public BitwardenConfigurationProvider(BitwardenConfigurationOptions options)
        : this(options, new BitwardenClientWrapper())
    {
        // EMPTY
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitwardenConfigurationProvider"/> class.
    /// This constructor is used for unit testing.
    /// </summary>
    /// <param name="options">The Bitwarden authentication and filtering options.</param>
    /// <param name="clientWrapper">The mockable client wrapper.</param>
    internal BitwardenConfigurationProvider(
        BitwardenConfigurationOptions options,
        IBitwardenClientWrapper clientWrapper
    )
    {
        _options = options;
        _clientWrapper = clientWrapper;
    }

    /// <summary>
    /// Loads secrets from the Bitwarden Secrets Manager into the configuration <see cref="ConfigurationProvider.Data"/> dictionary.
    /// </summary>
    /// <remarks>
    /// The method performs the following steps:
    /// <list type="number">
    ///   <item>Authenticates via Machine Account Access Token.</item>
    ///   <item>Lists all secret identifiers for the configured project.</item>
    ///   <item>Fetches full secret values in a single batch call (<c>GetByIds</c>).</item>
    ///   <item>Translates Bitwarden key separators (<c>__</c>) to .NET separators (<c>:</c>).</item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Bitwarden API call fails, preventing the application from starting with
    /// an incomplete or insecure configuration.
    /// </exception>
    public override void Load()
    {
        try
        {
            using (_clientWrapper)
            {
                _clientWrapper.Authenticate(_options.AccessToken);

                var identifiers = _clientWrapper.ListSecrets(_options.ProjectId).ToList();

                if (identifiers.Count == 0)
                {
                    Debug.WriteLine(
                        "[Bitwarden] No secrets found for the project. Configuration will be empty."
                    );
                    return;
                }

                Guid[] projectIds = [.. identifiers.Select(static s => s.Id)];

                IEnumerable<BitwardenSecret> projectSecrets = _clientWrapper.GetSecretsByIds(projectIds);

                foreach (BitwardenSecret secret in projectSecrets)
                {
                    // Translate Bitwarden hierarchy separator (__) to .NET convention (:).
                    // Example: "JwtSettings__SecretKey" -> "JwtSettings:SecretKey"
                    string configKey = secret.Key.Replace("__", ":", StringComparison.Ordinal);
                    Data[configKey] = secret.Value;
                }

                Debug.WriteLine($"[Bitwarden] {Data.Count} secret(s) loaded successfully.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load secrets from Bitwarden Secrets Manager. "
                    + $"Verify that BITWARDEN_ACCESS_TOKEN, BITWARDEN_ORGANIZATION_ID, and BITWARDEN_PROJECT_ID "
                    + $"are correctly set. Inner error: {ex.Message}",
                ex
            );
        }
    }
}
