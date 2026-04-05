// <copyright file="RefreshTokenDbRecordConfiguration.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Entity Framework configuration for the RefreshTokenDbRecord entity.
/// </summary>
internal class RefreshTokenDbRecordConfiguration : IEntityTypeConfiguration<RefreshTokenDbRecord>
{
    public void Configure(EntityTypeBuilder<RefreshTokenDbRecord> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        // Required fields and size constraints
        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(256); // SHA256 base64 is 44 chars, 256 is safe enough

        builder.Property(rt => rt.CreatedByIp).IsRequired().HasMaxLength(45); // Max length for an IPv6 address

        builder.Property(rt => rt.ExpiresAt).IsRequired();

        // Foreign Key relation to User
        builder
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade); // If user is deleted, kill the tokens

        // Index for fast lookups during token rotation or revocation
        builder.HasIndex(rt => new { rt.UserId, rt.TokenHash }).IsUnique();
    }
}
