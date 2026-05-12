// <copyright file="IBitwardenClientWrapper.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Core.Infra.Configuration;

/// <summary>
/// Defines a wrapper for the Bitwarden SDK client to enable unit testing and decoupling.
/// </summary>
internal interface IBitwardenClientWrapper : IDisposable
{
    /// <summary>
    /// Authenticates with the Bitwarden Secrets Manager using an access token.
    /// </summary>
    /// <param name="accessToken">The machine account access token.</param>
    void Authenticate(string accessToken);

    /// <summary>
    /// Lists all secret identifiers for the specified organization.
    /// </summary>
    /// <param name="organizationId">The organization GUID.</param>
    /// <returns>A collection of secret identifiers.</returns>
    IEnumerable<BitwardenSecretIdentifier> ListSecrets(Guid organizationId);

    /// <summary>
    /// Fetches full secret data for the specified IDs in batch.
    /// </summary>
    /// <param name="ids">The array of secret GUIDs.</param>
    /// <returns>A collection of full secret details.</returns>
    IEnumerable<BitwardenSecret> GetSecretsByIds(Guid[] ids);
}
