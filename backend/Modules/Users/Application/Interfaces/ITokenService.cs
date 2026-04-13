// <copyright file="ITokenService.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Modules.Users.Application.Interfaces;

/// <summary>
/// Service responsible for handling JWT and Refresh Token lifecycles.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a new JWT and Refresh token pair for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting token generation.</param>
    /// <param name="profileId">The profile identifier associated with the authenticated user.</param>
    /// <param name="orgId">The organization identifier associated with the authenticated user.</param>
    /// <param name="impersonatorId">The optional identifier of the actor performing impersonation.</param>
    /// <param name="ipAddress">The client IP address used to bind and track the token session.</param>
    /// <returns>A task that resolves to the generated access and refresh token pair response.</returns>
    Task<TokenResponse> GenerateTokenPairAsync(
        Guid userId,
        Guid profileId,
        Guid orgId,
        Guid? impersonatorId,
        string ipAddress
    );

    /// <summary>
    /// Revokes an active token pair, instantly killing the session in cache and database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose token pair will be revoked.</param>
    /// <param name="jti">The unique JWT identifier (JTI) of the access token to revoke.</param>
    /// <param name="refreshToken">The refresh token string associated with the session to revoke.</param>
    /// <returns>A task that represents the asynchronous token revocation operation.</returns>
    Task RevokeTokensAsync(Guid userId, string jti, string refreshToken);
}

/// <summary>
/// DTO containing the generated access and refresh tokens.
/// </summary>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string AccessId
);
