// <copyright file="UserDbRecord.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Core.Infra.Persistence.Models;

namespace EleveRats.Modules.Users.Infra.Persistence.Models;

/// <summary>
/// Supported Single Sign-On providers.
/// </summary>
internal enum SsoProvider
{
    None = 0,
    Google = 1,
    Apple = 2,
    Microsoft = 3,
}

/// <summary>
/// Database record for the User entity.
/// Anemic model meant exclusively for Entity Framework Core mapping.
/// </summary>
internal sealed class UserDbRecord : AuditableDbRecord
{
    /// <summary>
    /// Primary key. Generated using UUID v7 for sequential database indexing.
    /// </summary>
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? Phone { get; set; }

    public bool IsMaster { get; set; }

    public string? ExternalSsoCode { get; set; }

    public SsoProvider? ExternalSso { get; set; }
}
