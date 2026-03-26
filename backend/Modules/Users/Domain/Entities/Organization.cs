// <copyright file="Organization.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Modules.Users.Domain.Entities;

/// <summary>
/// Represents an Organization, which serves as the tenant boundary in the system.
/// </summary>
internal class Organization
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Organization"/> class.
    /// Private constructor to enforce factory methods.
    /// </summary>
    private Organization(
        Guid id,
        string name,
        bool isActive,
        DateTimeOffset createdAt,
        string createdBy,
        DateTimeOffset? updatedAt = null,
        string? updatedBy = null
    )
    {
        Id = id;
        Name = name;
        IsActive = isActive;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Gets the unique identifier (UUID v7).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the organization.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the organization is active.
    /// </summary>
    public bool IsActive { get; private set; }

    // --- Audit Metadata ---
    public DateTimeOffset CreatedAt { get; private set; }

    public string CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }

    /// <summary>
    /// Factory method to CREATE a brand new Organization.
    /// Generates a new UUID v7 and enforces initial invariants.
    /// </summary>
    /// <param name="name">The name of the organization.</param>
    /// <param name="createdBy">The user who created the organization.</param>
    /// <returns>A new instance of the <see cref="Organization"/> class.</returns>
    public static Organization Create(string name, string createdBy)
    {
        ValidateName(name);

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("Creator context is required.", nameof(createdBy));
        }

        return new Organization(
            id: Guid.CreateVersion7(),
            name: name.Trim(),
            isActive: true,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: createdBy
        );
    }

    /// <summary>
    /// Factory method to RECONSTITUTE an existing Organization from persistence.
    /// </summary>
    /// <param name="id">The unique identifier of the organization.</param>
    /// <param name="name">The name of the organization.</param>
    /// <param name="isActive">A value indicating whether the organization is active.</param>
    /// <param name="createdAt">The date and time when the organization was created.</param>
    /// <param name="createdBy">The user who created the organization.</param>
    /// <param name="updatedAt">The date and time when the organization was last updated.</param>
    /// <param name="updatedBy">The user who last updated the organization.</param>
    /// <returns>A new instance of the <see cref="Organization"/> class.</returns>
    public static Organization Reconstitute(
        Guid id,
        string name,
        bool isActive,
        DateTimeOffset createdAt,
        string createdBy,
        DateTimeOffset? updatedAt,
        string? updatedBy
    ) => new(id, name, isActive, createdAt, createdBy, updatedAt, updatedBy);

    // --- Domain Behaviors ---

    /// <summary>
    /// Renames the organization.
    /// </summary>
    /// <param name="newName">The new name of the organization.</param>
    /// <param name="updatedBy">The user who updated the organization.</param>
    public void Rename(string newName, string updatedBy)
    {
        ValidateName(newName);
        Name = newName.Trim();
        RecordUpdate(updatedBy);
    }

    /// <summary>
    /// Deactivates the organization, locking out all associated profiles.
    /// </summary>
    /// <param name="updatedBy">The user who updated the organization.</param>
    public void Deactivate(string updatedBy)
    {
        if (!IsActive)
        {
            return; // Idempotent operation
        }

        IsActive = false;
        RecordUpdate(updatedBy);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Organization name cannot be empty.", nameof(name));
        }

        if (name.Length < 3)
        {
            throw new ArgumentException(
                "Organization name must be at least 3 characters long.",
                nameof(name)
            );
        }
    }

    // --- Private Helpers ---
    private void RecordUpdate(string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
        {
            throw new ArgumentException("Updater context is required.", nameof(updatedBy));
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }
}
