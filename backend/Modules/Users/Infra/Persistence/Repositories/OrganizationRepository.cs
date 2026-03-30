// <copyright file="OrganizationRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Entity Framework Core implementation of the IOrganizationRepository.
/// Handles the mapping between the rich Domain Entity and the anemic Database Record.
/// </summary>
internal sealed class OrganizationRepository(UsersDbContext dbContext) : IOrganizationRepository
{
    private readonly UsersDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Organization?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        OrganizationDbRecord? record = await _dbContext
            .Organizations.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return record is null ? null : MapToDomain(record);
    }

    public async Task AddAsync(
        Organization organization,
        CancellationToken cancellationToken = default
    )
    {
        OrganizationDbRecord record = MapToRecord(organization);

        // Note: We only add to the context. Saving changes is the UnitOfWork's responsibility.
        await _dbContext.Organizations.AddAsync(record, cancellationToken);
    }

    public void Update(Organization organization)
    {
        OrganizationDbRecord record = MapToRecord(organization);

        // Note: EF Core tracks by ID. Update marks the entity state as Modified.
        _dbContext.Organizations.Update(record);
    }

    /// <summary>
    /// Maps an anemic OrganizationDbRecord to a rich Organization domain entity.
    /// </summary>
    private static Organization MapToDomain(OrganizationDbRecord record)
    {
        return Organization.Reconstitute(
            id: record.Id,
            name: record.Name,
            isActive: true, // If it was loaded, it's not soft-deleted (Global Filter)
            createdAt: record.CreatedAt,
            createdBy: record.CreatedBy,
            updatedAt: record.UpdatedAt,
            updatedBy: record.UpdatedBy
        );
    }

    /// <summary>
    /// Maps a rich Organization domain entity down to an anemic OrganizationDbRecord for EF Core.
    /// </summary>
    private static OrganizationDbRecord MapToRecord(Organization organization)
    {
        return new OrganizationDbRecord
        {
            Id = organization.Id,
            Name = organization.Name,
            CreatedAt = organization.CreatedAt,
            CreatedBy = organization.CreatedBy,
            UpdatedAt = organization.UpdatedAt,
            UpdatedBy = organization.UpdatedBy,
        };
    }
}
