// <copyright file="ProfileDbRecord.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Domain.Enums;

namespace EleveRats.Modules.Users.Infra.Persistence.Models;

/// <summary>
/// Database record for the Profile entity, linking a User to an Organization.
/// </summary>
internal sealed class ProfileDbRecord : AuditableDbRecord
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public Gender Gender { get; set; }

    public bool IsMember { get; set; }

    public ProfileType ProfileType { get; set; }

    // --- Navigation Properties ---
    public OrganizationDbRecord? Organization { get; set; }

    public UserDbRecord? User { get; set; }
}
