// <copyright file="BitwardenConfigurationOptions.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Core.Infra.Configuration;

/// <summary>
/// Immutable options record for the Bitwarden Secrets Manager configuration provider.
/// All credentials are expected to be sourced from environment variables for security compliance.
/// </summary>
internal sealed record BitwardenConfigurationOptions
{
    /// <summary>
    /// Gets the Machine Account Access Token used to authenticate with the Bitwarden Secrets Manager API.
    /// Sourced from the <c>BITWARDEN_ACCESS_TOKEN</c> environment variable.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Bitwarden Organization ID that owns the secrets to be fetched.
    /// Sourced from the <c>BITWARDEN_ORGANIZATION_ID</c> environment variable.
    /// </summary>
    public Guid OrganizationId { get; init; }

    /// <summary>
    /// Gets the Bitwarden Project ID used to filter secrets.
    /// Sourced from the <c>BITWARDEN_PROJECT_ID</c> environment variable.
    /// </summary>
    public Guid ProjectId { get; init; }
}
