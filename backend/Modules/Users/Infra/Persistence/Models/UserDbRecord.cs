// <copyright file="UserDbRecord.cs" company="PlaceholderCompany">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// </copyright>

namespace EleveRats.Modules.Users.Infra.Persistence.Models;

using System;

/// <summary>
/// Supported Single Sign-On providers.
/// </summary>
public enum SsoProvider
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
public class UserDbRecord
{
    /// <summary>
    /// Primary key. Generated using UUID v7 for sequential database indexing.
    /// </summary>
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsMaster { get; set; }

    public string? ExternalSsoCode { get; set; }

    public SsoProvider? ExternalSso { get; set; }

    // --- Audit Trails ---
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// JSON payload containing audit context (e.g., traceId, userId, serviceName).
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// JSON payload containing audit context for updates.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null means the record is active.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// JSON payload containing audit context for deletions.
    /// </summary>
    public string? DeletedBy { get; set; }
}
