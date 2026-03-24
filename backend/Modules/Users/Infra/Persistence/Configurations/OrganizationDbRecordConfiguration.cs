// <copyright file="OrganizationDbRecordConfiguration.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using EleveRats.Modules.Users.Infra.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EleveRats.Modules.Users.Infra.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Organization database record.
/// </summary>
internal sealed class OrganizationDbRecordConfiguration : IEntityTypeConfiguration<OrganizationDbRecord>
{
    public void Configure(EntityTypeBuilder<OrganizationDbRecord> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        // --- Audit Trails (JSONB) ---
        builder.Property(x => x.CreatedBy).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.UpdatedBy).HasColumnType("jsonb");
        builder.Property(x => x.DeletedBy).HasColumnType("jsonb");

        // --- Global Query Filter ---
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
