// <copyright file="AuditableDbRecord.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Core.Infra.Persistence.Models;

/// <summary>
/// Abstract base class providing standard audit trail properties for database entities.
/// </summary>
internal abstract class AuditableDbRecord
{
    /// <summary>
    /// Timestamp of when the record was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the record.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the last update.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp of when the record was soft-deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Identifier of the user who soft-deleted the record.
    /// </summary>
    public string? DeletedBy { get; set; }
}
