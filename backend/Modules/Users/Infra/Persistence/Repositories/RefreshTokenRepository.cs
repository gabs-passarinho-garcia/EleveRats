// <copyright file="RefreshTokenRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Domain.Entities;
using EleveRats.Modules.Users.Infra.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EleveRats.Modules.Users.Infra.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the <see cref="IRefreshTokenRepository"/>.
/// Handles the mapping between the rich <see cref="RefreshToken"/> domain entity
/// and the anemic <see cref="RefreshTokenDbRecord"/> persistence model.
/// </summary>
internal sealed class RefreshTokenRepository(UsersDbContext dbContext) : IRefreshTokenRepository
{
    private readonly UsersDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <inheritdoc/>
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        RefreshTokenDbRecord record = MapToRecord(token);
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> FindByUserAndHashAsync(
        Guid userId,
        string tokenHash,
        CancellationToken cancellationToken = default
    )
    {
        RefreshTokenDbRecord? record = await _dbContext
            .Set<RefreshTokenDbRecord>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rt => rt.UserId == userId && rt.TokenHash == tokenHash,
                cancellationToken
            );

        return record is null ? null : MapToDomain(record);
    }

    /// <inheritdoc/>
    public void Update(RefreshToken token)
    {
        RefreshTokenDbRecord record = MapToRecord(token);
        _dbContext.Set<RefreshTokenDbRecord>().Update(record);
    }

    /// <summary>
    /// Maps a rich <see cref="RefreshToken"/> domain entity to the anemic
    /// <see cref="RefreshTokenDbRecord"/> for EF Core persistence.
    /// </summary>
    private static RefreshTokenDbRecord MapToRecord(RefreshToken token) =>
        new()
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            ExpiresAt = token.ExpiresAt,
            CreatedByIp = token.CreatedByIp,
            RevokedAt = token.RevokedAt,
        };

    /// <summary>
    /// Maps an anemic <see cref="RefreshTokenDbRecord"/> to a rich
    /// <see cref="RefreshToken"/> domain entity via the Reconstitute factory method.
    /// </summary>
    private static RefreshToken MapToDomain(RefreshTokenDbRecord record) =>
        RefreshToken.Reconstitute(
            id: record.Id,
            userId: record.UserId,
            tokenHash: record.TokenHash,
            expiresAt: record.ExpiresAt,
            createdByIp: record.CreatedByIp,
            revokedAt: record.RevokedAt
        );
}
