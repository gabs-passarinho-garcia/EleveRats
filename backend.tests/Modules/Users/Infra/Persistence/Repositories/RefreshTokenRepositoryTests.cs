// <copyright file="RefreshTokenRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Modules.Users.Domain.Entities;
using EleveRats.Modules.Users.Infra.Persistence;
using EleveRats.Modules.Users.Infra.Persistence.Models;
using EleveRats.Modules.Users.Infra.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Infra.Persistence.Repositories;

/// <summary>
/// Unit tests for the <see cref="RefreshTokenRepository"/>.
/// Uses EF Core In-Memory database to verify persistence, domain-to-record
/// mapping, and query behavior without hitting a real database.
/// </summary>
public sealed class RefreshTokenRepositoryTests : IDisposable
{
    private readonly UsersDbContext _dbContext;
    private readonly RefreshTokenRepository _repository;

    private static readonly Guid _userId = Guid.CreateVersion7();
    private const string _tokenHash = "sha256_hash_of_raw_token";
    private const string _ipAddress = "10.0.0.1";

    public RefreshTokenRepositoryTests()
    {
        DbContextOptions<UsersDbContext> options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new UsersDbContext(options);
        _repository = new RefreshTokenRepository(_dbContext);
    }

    // -----------------------------------------------------------------------
    // AddAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_ShouldTrackRecordInContext_WithoutCommitting()
    {
        // Arrange
        RefreshToken token = RefreshToken.Create(
            _userId,
            _tokenHash,
            DateTime.UtcNow.AddDays(7),
            _ipAddress
        );

        // Act
        await _repository.AddAsync(token);

        // Assert — record must be tracked locally but NOT yet saved to DB
        RefreshTokenDbRecord? tracked = _dbContext
            .Set<RefreshTokenDbRecord>()
            .Local.FirstOrDefault(r => r.Id == token.Id);

        tracked.Should().NotBeNull();
        tracked!.UserId.Should().Be(_userId);
        tracked.TokenHash.Should().Be(_tokenHash);
        tracked.CreatedByIp.Should().Be(_ipAddress);
        tracked.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldMapAllDomainPropertiesToRecord()
    {
        // Arrange
        DateTime expiresAt = DateTime.UtcNow.AddDays(14);
        RefreshToken token = RefreshToken.Create(_userId, _tokenHash, expiresAt, _ipAddress);

        // Act
        await _repository.AddAsync(token);
        await _dbContext.SaveChangesAsync();

        // Assert — verify field-by-field mapping after persisting
        RefreshTokenDbRecord? persisted = await _dbContext
            .Set<RefreshTokenDbRecord>()
            .FindAsync(token.Id);

        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(token.Id);
        persisted.UserId.Should().Be(token.UserId);
        persisted.TokenHash.Should().Be(token.TokenHash);
        persisted.ExpiresAt.Should().Be(token.ExpiresAt);
        persisted.CreatedByIp.Should().Be(token.CreatedByIp);
        persisted.RevokedAt.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // FindByUserAndHashAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FindByUserAndHashAsync_WhenActiveTokenExists_ShouldReturnMappedDomainEntity()
    {
        // Arrange
        var record = new RefreshTokenDbRecord
        {
            Id = Guid.CreateVersion7(),
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = _ipAddress,
            RevokedAt = null,
        };
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        RefreshToken? result = await _repository.FindByUserAndHashAsync(_userId, _tokenHash);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
        result.UserId.Should().Be(_userId);
        result.TokenHash.Should().Be(_tokenHash);
        result.CreatedByIp.Should().Be(_ipAddress);
        result.RevokedAt.Should().BeNull();
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task FindByUserAndHashAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Act
        RefreshToken? result = await _repository.FindByUserAndHashAsync(
            Guid.NewGuid(),
            "nonexistent_hash"
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByUserAndHashAsync_WhenUserIdDoesNotMatch_ShouldReturnNull()
    {
        // Arrange — same hash, different user
        var record = new RefreshTokenDbRecord
        {
            Id = Guid.CreateVersion7(),
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = _ipAddress,
        };
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act — query with a different userId
        RefreshToken? result = await _repository.FindByUserAndHashAsync(Guid.NewGuid(), _tokenHash);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByUserAndHashAsync_WhenTokenIsRevoked_ShouldReturnEntityWithRevokedAtSet()
    {
        // Arrange
        DateTime revokedAt = DateTime.UtcNow.AddMinutes(-30);
        var record = new RefreshTokenDbRecord
        {
            Id = Guid.CreateVersion7(),
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = _ipAddress,
            RevokedAt = revokedAt,
        };
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        RefreshToken? result = await _repository.FindByUserAndHashAsync(_userId, _tokenHash);

        // Assert — revoked tokens are still returned (caller decides what to do)
        result.Should().NotBeNull();
        result!.RevokedAt.Should().Be(revokedAt);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task FindByUserAndHashAsync_WhenTokenIsExpired_ShouldReturnEntityWithIsExpiredTrue()
    {
        // Arrange
        var record = new RefreshTokenDbRecord
        {
            Id = Guid.CreateVersion7(),
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = DateTime.UtcNow.AddSeconds(-10), // past
            CreatedByIp = _ipAddress,
            RevokedAt = null,
        };
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        RefreshToken? result = await _repository.FindByUserAndHashAsync(_userId, _tokenHash);

        // Assert — expired tokens are returned; the domain entity correctly reports IsExpired
        result.Should().NotBeNull();
        result!.IsExpired.Should().BeTrue();
        result.IsActive.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_ShouldMarkRecordAsModifiedWithNewRevokedAt()
    {
        // Arrange — seed a record, then detach to simulate a fresh reconstitution
        var id = Guid.CreateVersion7();
        DateTime expiresAt = DateTime.UtcNow.AddDays(7);

        var record = new RefreshTokenDbRecord
        {
            Id = id,
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = _ipAddress,
            RevokedAt = null,
        };
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(record).State = EntityState.Detached;

        // Reconstitute domain entity with revocation
        DateTime revokedAt = DateTime.UtcNow;
        RefreshToken updatedToken = RefreshToken.Reconstitute(
            id,
            _userId,
            _tokenHash,
            expiresAt,
            _ipAddress,
            revokedAt
        );

        // Act
        _repository.Update(updatedToken);

        // Assert
        RefreshTokenDbRecord? tracked = _dbContext
            .Set<RefreshTokenDbRecord>()
            .Local.FirstOrDefault(r => r.Id == id);

        tracked.Should().NotBeNull();
        tracked!.RevokedAt.Should().Be(revokedAt);
        _dbContext.Entry(tracked).State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task Update_AfterSaveChanges_ShouldPersistRevokedAtToDatabase()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        DateTime expiresAt = DateTime.UtcNow.AddDays(7);

        var record = new RefreshTokenDbRecord
        {
            Id = id,
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = _ipAddress,
        };
        await _dbContext.Set<RefreshTokenDbRecord>().AddAsync(record);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(record).State = EntityState.Detached;

        DateTime revokedAt = DateTime.UtcNow;
        RefreshToken revokedToken = RefreshToken.Reconstitute(
            id,
            _userId,
            _tokenHash,
            expiresAt,
            _ipAddress,
            revokedAt
        );

        // Act
        _repository.Update(revokedToken);
        await _dbContext.SaveChangesAsync();

        // Assert — read directly from DB to confirm durability
        _dbContext
            .Entry(await _dbContext.Set<RefreshTokenDbRecord>().FindAsync(id) ?? new())
            .State = EntityState.Detached;
        RefreshTokenDbRecord? persisted = await _dbContext
            .Set<RefreshTokenDbRecord>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        persisted.Should().NotBeNull();
        persisted!.RevokedAt.Should().Be(revokedAt);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
