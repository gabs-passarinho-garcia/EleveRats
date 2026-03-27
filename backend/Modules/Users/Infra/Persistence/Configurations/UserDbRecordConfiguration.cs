// <copyright file="UserDbRecordConfiguration.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EleveRats.Modules.Users.Infra.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the User database record.
/// Maps the C# properties to PostgreSQL specific columns and constraints.
/// </summary>
internal sealed class UserDbRecordConfiguration : IEntityTypeConfiguration<UserDbRecord>
{
    public void Configure(EntityTypeBuilder<UserDbRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Table name definition
        builder.ToTable("users");

        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties and constraints
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);

        // Ensures no two users can register with the same email
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(x => x.Phone).HasMaxLength(20);

        builder.Property(x => x.IsMaster).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.ExternalSsoCode).HasMaxLength(255);

        // Stores the Enum as an integer in the database for better performance
        builder.Property(x => x.ExternalSso).HasConversion<int>();

        // --- Audit Trails (The JSONB Magic) ---
        builder.Property(x => x.CreatedBy).HasColumnType("jsonb").IsRequired();

        builder.Property(x => x.UpdatedBy).HasColumnType("jsonb");

        builder.Property(x => x.DeletedBy).HasColumnType("jsonb");

        // --- Global Query Filter ---
        // Automatically excludes soft-deleted records from all LINQ queries
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
