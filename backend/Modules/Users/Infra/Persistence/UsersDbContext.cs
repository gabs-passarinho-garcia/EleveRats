// <copyright file="UsersDbContext.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Infra.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EleveRats.Modules.Users.Infra.Persistence;

/// <summary>
/// Database context scoped specifically to the Users module.
/// </summary>
internal sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Represents the users table in the database.
    /// </summary>
    internal DbSet<UserDbRecord> Users => Set<UserDbRecord>();

    /// <summary>
    /// Represents the profiles table in the database.
    /// </summary>
    internal DbSet<ProfileDbRecord> Profiles => Set<ProfileDbRecord>();

    /// <summary>
    /// Represents the organizations table in the database.
    /// </summary>
    internal DbSet<OrganizationDbRecord> Organizations => Set<OrganizationDbRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        // This single line acts as a scout. It scans the current assembly, finds all
        // configurations (User, Organization, Profile) and applies them.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
