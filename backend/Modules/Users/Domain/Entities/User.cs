// <copyright file="User.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Text.RegularExpressions;

namespace EleveRats.Modules.Users.Domain.Entities;

/// <summary>
/// Represents the root User entity in the identity domain.
/// Encapsulates authentication credentials and global user state.
/// </summary>
internal partial class User
{
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// Private constructor enforces object creation via factory methods only.
    /// </summary>
    private User(
        Guid id,
        string email,
        string passwordHash,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null
    )
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Gets the unique identifier (UUID v7).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the user's email address, which serves as the login.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Gets the hashed password for authentication.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user is active and allowed to log in.
    /// </summary>
    public bool IsActive { get; private set; }

    // --- Audit Metadata ---
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when the user was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Factory method to CREATE a brand new User.
    /// Generates a new UUID v7 and validates domain invariants.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The pre-hashed password.</param>
    /// <returns>A new, active User instance.</returns>
    public static User Create(string email, string passwordHash)
    {
        ValidateEmail(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        }

        return new User(
            id: Guid.CreateVersion7(),
            email: email.ToUpperInvariant(),
            passwordHash: passwordHash,
            isActive: true,
            createdAt: DateTimeOffset.UtcNow
        );
    }

    /// <summary>
    /// Factory method to RECONSTITUTE an existing User from the database.
    /// Bypasses domain creation rules and respects existing persisted state.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The pre-hashed password.</param>
    /// <param name="isActive">A value indicating whether the user is active and allowed to log in.</param>
    /// <param name="createdAt">The date and time when the user was created.</param>
    /// <param name="updatedAt">The date and time when the user was last updated.</param>
    /// <returns>A new, active User instance.</returns>
    public static User Reconstitute(
        Guid id,
        string email,
        string passwordHash,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt
    ) => new(id, email, passwordHash, isActive, createdAt, updatedAt);

    // --- Domain Behaviors ---

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException(
                "New password hash cannot be empty.",
                nameof(newPasswordHash)
            );
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Validates the email format using a compiled Regex.
    /// </summary>
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        if (!EmailRegex().IsMatch(email))
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
