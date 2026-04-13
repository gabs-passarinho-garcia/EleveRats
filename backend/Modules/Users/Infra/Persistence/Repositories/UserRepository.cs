// <copyright file="UserRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Entity Framework Core implementation of the IUserRepository.
/// Handles the mapping between the rich Domain Entity and the anemic Database Record.
/// </summary>
internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
    private readonly UsersDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        UserDbRecord? record = await _dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return record is null ? null : MapToDomain(record);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        UserDbRecord? record = await _dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return record is null ? null : MapToDomain(record);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        UserDbRecord record = MapToRecord(user);

        // Note: We only add to the context. Saving changes is the UnitOfWork's responsibility.
        await _dbContext.Users.AddAsync(record, cancellationToken);
    }

    public void Update(User user)
    {
        UserDbRecord record = MapToRecord(user);

        // Note: EF Core tracks by ID. Update marks the entity state as Modified.
        _dbContext.Users.Update(record);
    }

    /// <summary>
    /// Maps an anemic UserDbRecord to a rich User domain entity.
    /// Uses the factory method to bypass domain creation rules and respect persisted state.
    /// </summary>
    private static User MapToDomain(UserDbRecord record)
    {
        return User.Reconstitute(
            id: record.Id,
            email: record.Email,
            passwordHash: record.PasswordHash,
            isActive: record.IsActive,
            isMaster: record.IsMaster,
            phone: record.Phone,
            externalSsoCode: record.ExternalSsoCode,
            externalSso: record.ExternalSso,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt
        );
    }

    /// <summary>
    /// Maps a rich User domain entity down to an anemic UserDbRecord for EF Core.
    /// </summary>
    private static UserDbRecord MapToRecord(User user)
    {
        return new UserDbRecord
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            IsActive = user.IsActive,
            IsMaster = user.IsMaster,
            Phone = user.Phone,
            ExternalSsoCode = user.ExternalSsoCode,
            ExternalSso = user.ExternalSso,
        };
    }
}
