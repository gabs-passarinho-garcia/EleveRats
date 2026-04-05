// <copyright file="RefreshTokenDbRecord.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using EleveRats.Core.Infra.Persistence.Models;

namespace EleveRats.Modules.Users.Infra.Persistence.Models;

/// <summary>
/// Represents a refresh token entity for maintaining long-lived user sessions.
/// </summary>
internal class RefreshTokenDbRecord : AuditableDbRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string CreatedByIp { get; set; } = string.Empty;

    public DateTime? RevokedAt { get; set; }

    public UserDbRecord User { get; set; } = null!;

    /// <summary>
    /// Evaluates if the current token is active and valid.
    /// </summary>
    public bool IsActive => RevokedAt == null && !IsExpired;

    /// <summary>
    /// Evaluates if the current token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
