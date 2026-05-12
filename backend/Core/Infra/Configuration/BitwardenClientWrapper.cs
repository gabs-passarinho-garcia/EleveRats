// <copyright file="BitwardenClientWrapper.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using Bitwarden.Sdk;

namespace EleveRats.Core.Infra.Configuration;

/// <summary>
/// A wrapper for the Bitwarden SDK to facilitate unit testing.
/// </summary>
internal sealed class BitwardenClientWrapper : IBitwardenClientWrapper
{
    private BitwardenClient? _client;

    /// <inheritdoc/>
    public void Authenticate(string accessToken)
    {
        _client = new BitwardenClient();
        _client.Auth.LoginAccessToken(accessToken);
    }

    /// <inheritdoc/>
    public IEnumerable<BitwardenSecretIdentifier> ListSecrets(Guid organizationId)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Client must be authenticated before use.");
        }

        SecretIdentifiersResponse response = _client.Secrets.List(organizationId);
        return response.Data.Select(static s => new BitwardenSecretIdentifier(s.Id, s.Key));
    }

    /// <inheritdoc/>
    public IEnumerable<BitwardenSecret> GetSecretsByIds(Guid[] ids)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Client must be authenticated before use.");
        }

        SecretsResponse response = _client.Secrets.GetByIds(ids);
        return response.Data.Select(static s => new BitwardenSecret(
            s.Id,
            s.Key,
            s.Value,
            s.ProjectId
        ));
    }

    /// <inheritdoc/>
    public void Dispose() => _client?.Dispose();
}
