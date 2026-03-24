// <copyright file="Profile.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Domain.Enums;

namespace EleveRats.Modules.Users.Domain.Entities;

/// <summary>
/// Represents the link between a User and an Organization.
/// Defines the specific details and access level a user has within a tenant.
/// </summary>
internal class Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> class.
    /// Private constructor to enforce factory methods.
    /// </summary>
    private Profile(
        Guid id,
        Guid organizationId,
        Guid userId,
        string fullName,
        int age,
        Gender gender,
        bool isMember,
        ProfileType profileType,
        DateTimeOffset createdAt,
        string createdBy,
        DateTimeOffset? updatedAt = null,
        string? updatedBy = null
    )
    {
        Id = id;
        OrganizationId = organizationId;
        UserId = userId;
        FullName = fullName;
        Age = age;
        Gender = gender;
        IsMember = isMember;
        ProfileType = profileType;
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
    /// Gets the ID of the Organization (Tenant) this profile belongs to.
    /// </summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Gets the ID of the global User this profile represents.
    /// </summary>
    public Guid UserId { get; private set; }

    public string FullName { get; private set; }

    public int Age { get; private set; }

    public Gender Gender { get; private set; }

    public bool IsMember { get; private set; }

    public ProfileType ProfileType { get; private set; }

    // --- Audit Metadata ---
    public DateTimeOffset CreatedAt { get; private set; }

    public string CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }

    /// <summary>
    /// Factory method to CREATE a brand new Profile.
    /// Enforces business invariants right at birth.
    /// </summary>
    /// <param name="organizationId">The ID of the Organization (Tenant) this profile belongs to.</param>
    /// <param name="userId">The ID of the global User this profile represents.</param>
    /// <param name="fullName">The full name of the user.</param>
    /// <param name="age">The age of the user.</param>
    /// <param name="gender">The gender of the user.</param>
    /// <param name="profileType">The type of profile.</param>
    /// <param name="createdBy">The user who created the profile.</param>
    /// <returns>A new <see cref="Profile"/> instance.</returns>
    public static Profile Create(
        Guid organizationId,
        Guid userId,
        string fullName,
        int age,
        Gender gender,
        ProfileType profileType,
        string createdBy
    )
    {
        ValidateFullName(fullName);
        ValidateAge(age);

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("Creator context is required.", nameof(createdBy));
        }

        return new Profile(
            id: Guid.CreateVersion7(),
            organizationId: organizationId,
            userId: userId,
            fullName: fullName.Trim(),
            age: age,
            gender: gender,
            isMember: true, // Defaulting to active member upon creation
            profileType: profileType,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: createdBy
        );
    }

    /// <summary>
    /// Factory method to RECONSTITUTE an existing Profile from persistence.
    /// Bypasses domain creation rules and respects existing persisted state.
    /// </summary>
    /// <param name="id">The unique identifier (UUID v7).</param>
    /// <param name="organizationId">The ID of the Organization (Tenant) this profile belongs to.</param>
    /// <param name="userId">The ID of the global User this profile represents.</param>
    /// <param name="fullName">The full name of the user.</param>
    /// <param name="age">The age of the user.</param>
    /// <param name="gender">The gender of the user.</param>
    /// <param name="isMember">A value indicating whether the user is a member.</param>
    /// <param name="profileType">The type of profile.</param>
    /// <param name="createdAt">The date and time when the profile was created.</param>
    /// <param name="createdBy">The user who created the profile.</param>
    /// <param name="updatedAt">The date and time when the profile was last updated.</param>
    /// <param name="updatedBy">The user who last updated the profile.</param>
    /// <returns>A new <see cref="Profile"/> instance.</returns>
    public static Profile Reconstitute(
        Guid id,
        Guid organizationId,
        Guid userId,
        string fullName,
        int age,
        Gender gender,
        bool isMember,
        ProfileType profileType,
        DateTimeOffset createdAt,
        string createdBy,
        DateTimeOffset? updatedAt,
        string? updatedBy
    )
    {
        return new Profile(
            id,
            organizationId,
            userId,
            fullName,
            age,
            gender,
            isMember,
            profileType,
            createdAt,
            createdBy,
            updatedAt,
            updatedBy
        );
    }

    // --- Domain Behaviors ---

    /// <summary>
    /// Updates the personal details of the profile.
    /// </summary>
    /// <param name="newFullName">The new full name of the user.</param>
    /// <param name="newAge">The new age of the user.</param>
    /// <param name="newGender">The new gender of the user.</param>
    /// <param name="updatedBy">The user who updated the profile.</param>
    public void UpdateDetails(string newFullName, int newAge, Gender newGender, string updatedBy)
    {
        ValidateFullName(newFullName);
        ValidateAge(newAge);

        FullName = newFullName.Trim();
        Age = newAge;
        Gender = newGender;

        RecordUpdate(updatedBy);
    }

    /// <summary>
    /// Changes the profile's access level within the organization.
    /// </summary>
    /// <param name="newProfileType">The new type of profile.</param>
    /// <param name="updatedBy">The user who updated the profile.</param>
    public void ChangeAccessLevel(ProfileType newProfileType, string updatedBy)
    {
        ProfileType = newProfileType;
        RecordUpdate(updatedBy);
    }

    /// <summary>
    /// Revokes membership, blocking access to the organization's resources.
    /// </summary>
    /// <param name="updatedBy">The user who updated the profile.</param>
    public void RevokeMembership(string updatedBy)
    {
        IsMember = false;
        RecordUpdate(updatedBy);
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        }

        if (fullName.Length < 3)
        {
            throw new ArgumentException(
                "Full name must be at least 3 characters long.",
                nameof(fullName)
            );
        }
    }

    private static void ValidateAge(int age)
    {
        if (age is < 0 or > 130)
        {
            throw new ArgumentException("Age must be a valid human age.", nameof(age));
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
