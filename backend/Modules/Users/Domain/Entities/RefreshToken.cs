// <copyright file="RefreshToken.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Modules.Users.Domain.Entities;

/// <summary>
/// Represents a persisted refresh token associated with a user session.
/// Encapsulates lifecycle rules such as expiration and revocation.
/// </summary>
internal sealed class RefreshToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// Private constructor enforces object creation via factory methods only.
    /// </summary>
    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string createdByIp,
        DateTime? revokedAt
    )
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        RevokedAt = revokedAt;
    }

    /// <summary>Gets the unique identifier of this token record.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the ID of the user who owns this token.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the hashed value of the raw refresh token string.</summary>
    public string TokenHash { get; private set; }

    /// <summary>Gets the UTC timestamp when this token expires.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>Gets the IP address from which this token was originally created.</summary>
    public string CreatedByIp { get; private set; }

    /// <summary>Gets the UTC timestamp when this token was revoked, if applicable.</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>Gets a value indicating whether the token is currently valid and not expired or revoked.</summary>
    public bool IsActive => RevokedAt is null && !IsExpired;

    /// <summary>Gets a value indicating whether the token has passed its expiration date.</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Factory method to CREATE a brand new RefreshToken for a user session.
    /// </summary>
    /// <param name="userId">The ID of the user who owns this token.</param>
    /// <param name="tokenHash">The SHA-256 hash of the raw token string.</param>
    /// <param name="expiresAt">The UTC expiry datetime for this token.</param>
    /// <param name="createdByIp">The IP address of the client making the request.</param>
    /// <returns>A new, active <see cref="RefreshToken"/> instance.</returns>
    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string createdByIp
    )
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));
        }

        if (string.IsNullOrWhiteSpace(createdByIp))
        {
            throw new ArgumentException("IP address cannot be empty.", nameof(createdByIp));
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Expiration date must be in the future.",
                nameof(expiresAt)
            );
        }

        return new RefreshToken(
            id: Guid.CreateVersion7(),
            userId: userId,
            tokenHash: tokenHash,
            expiresAt: expiresAt,
            createdByIp: createdByIp,
            revokedAt: null
        );
    }

    /// <summary>
    /// Factory method to RECONSTITUTE an existing RefreshToken from the database.
    /// Bypasses domain creation rules and respects persisted state.
    /// </summary>
    /// <param name="id">The unique identifier of this token record.</param>
    /// <param name="userId">The ID of the user who owns this token.</param>
    /// <param name="tokenHash">The SHA-256 hash of the raw token string.</param>
    /// <param name="expiresAt">The UTC expiry datetime for this token.</param>
    /// <param name="createdByIp">The IP address of the client making the request.</param>
    /// <param name="revokedAt">The UTC timestamp when this token was revoked, if applicable.</param>
    /// <returns>A new <see cref="RefreshToken"/> instance.</returns>
    public static RefreshToken Reconstitute(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string createdByIp,
        DateTime? revokedAt
    ) => new(id, userId, tokenHash, expiresAt, createdByIp, revokedAt);

    // --- Domain Behaviors ---

    /// <summary>
    /// Marks the token as revoked, invalidating it for further use.
    /// This is a no-op if the token is already revoked.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the token is already revoked.</exception>
    public void Revoke()
    {
        if (RevokedAt.HasValue)
        {
            throw new InvalidOperationException("This refresh token has already been revoked.");
        }

        RevokedAt = DateTime.UtcNow;
    }
}
