// <copyright file="ProfileRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Threading;
using System.Threading.Tasks;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Domain.Entities;
using EleveRats.Modules.Users.Infra.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EleveRats.Modules.Users.Infra.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the IProfileRepository.
/// Handles the mapping between the rich Domain Entity and the anemic Database Record.
/// </summary>
internal sealed class ProfileRepository(UsersDbContext dbContext) : IProfileRepository
{
    private readonly UsersDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ProfileDbRecord? record = await _dbContext
            .Profiles.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return record is null ? null : MapToDomain(record);
    }

    public async Task<Profile?> GetByUserIdAndOrganizationIdAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        ProfileDbRecord? record = await _dbContext
            .Profiles.AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.UserId == userId && u.OrganizationId == organizationId,
                cancellationToken
            );

        return record is null ? null : MapToDomain(record);
    }

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ProfileDbRecord record = MapToRecord(profile);

        // Note: Saving changes is the UnitOfWork's responsibility.
        await _dbContext.Profiles.AddAsync(record, cancellationToken);
    }

    public void Update(Profile profile)
    {
        ProfileDbRecord record = MapToRecord(profile);

        // Note: EF Core tracks by ID.
        _dbContext.Profiles.Update(record);
    }

    /// <summary>
    /// Maps an anemic ProfileDbRecord to a rich Profile domain entity.
    /// </summary>
    private static Profile MapToDomain(ProfileDbRecord record)
    {
        return Profile.Reconstitute(
            id: record.Id,
            organizationId: record.OrganizationId,
            userId: record.UserId,
            fullName: record.FullName,
            birthDate: record.BirthDate,
            gender: record.Gender,
            isMember: record.IsMember,
            profileType: record.ProfileType,
            createdAt: record.CreatedAt,
            createdBy: record.CreatedBy,
            updatedAt: record.UpdatedAt,
            updatedBy: record.UpdatedBy
        );
    }

    /// <summary>
    /// Maps a rich Profile domain entity down to an anemic ProfileDbRecord for EF Core.
    /// </summary>
    private static ProfileDbRecord MapToRecord(Profile profile)
    {
        return new ProfileDbRecord
        {
            Id = profile.Id,
            OrganizationId = profile.OrganizationId,
            UserId = profile.UserId,
            FullName = profile.FullName,
            BirthDate = profile.BirthDate,
            Gender = profile.Gender,
            IsMember = profile.IsMember,
            ProfileType = profile.ProfileType,
            CreatedAt = profile.CreatedAt,
            CreatedBy = profile.CreatedBy,
            UpdatedAt = profile.UpdatedAt,
            UpdatedBy = profile.UpdatedBy,
        };
    }
}
