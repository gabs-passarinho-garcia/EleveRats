// <copyright file="ProfileDbRecordConfiguration.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Entity Framework Core configuration for the Profile database record.
/// </summary>
internal sealed class ProfileDbRecordConfiguration : IEntityTypeConfiguration<ProfileDbRecord>
{
    public void Configure(EntityTypeBuilder<ProfileDbRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(255);

        builder.Property(x => x.BirthDate).IsRequired();

        builder.Property(x => x.ProfileType).HasConversion<int>().IsRequired();
        builder.Property(x => x.Gender).HasConversion<int>().IsRequired();

        // --- Audit Trails (JSONB) ---
        builder.Property(x => x.CreatedBy).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.UpdatedBy).HasColumnType("jsonb");
        builder.Property(x => x.DeletedBy).HasColumnType("jsonb");

        // --- Relationships (Foreign Keys) ---

        // A Profile belongs to an Organization. Cascade delete for tenant cleanup.
        builder
            .HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // A Profile belongs to a User. Cascade delete for global user cleanup.
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Global Query Filter ---
        // Excludes soft-deleted profiles
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
