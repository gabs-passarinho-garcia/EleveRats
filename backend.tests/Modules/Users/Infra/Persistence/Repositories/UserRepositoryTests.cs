// <copyright file="UserRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
/// Unit tests for the <see cref="UserRepository"/>.
/// Uses EF Core In-Memory database to verify persistence and mapping logic.
/// </summary>
public sealed class UserRepositoryTests : IDisposable
{
    private readonly UsersDbContext _dbContext;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        // Setup In-Memory database unique for each test instance
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new UsersDbContext(options);
        _repository = new UserRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnMappedDomainEntity()
    {
        // Arrange
        var record = new UserDbRecord
        {
            Id = Guid.CreateVersion7(),
            Email = "TEST@ELEVERATS.COM",
            PasswordHash = "hashed",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await _dbContext.Users.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        User? result = await _repository.GetByIdAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
        result.Email.Should().Be(record.Email);
        result.PasswordHash.Should().Be(record.PasswordHash);
        result.IsActive.Should().Be(record.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        User? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnMappedDomainEntity()
    {
        // Arrange
        string email = "FIND@ME.COM";
        var record = new UserDbRecord
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            PasswordHash = "hashed",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await _dbContext.Users.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        User? result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRecordToDbContext()
    {
        // Arrange
        var user = User.Create("new@test.com", "hash");

        // Act
        await _repository.AddAsync(user);

        // Assert
        // Repository should not save changes, just add to context
        UserDbRecord? record = _dbContext.Users.Local.FirstOrDefault(x => x.Id == user.Id);
        record.Should().NotBeNull();

        record!.Id.Should().Be(user.Id);
        record.Email.Should().Be(user.Email);
    }

    [Fact]
    public void Update_ShouldChangeEntityStateToModified()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var record = new UserDbRecord
        {
            Id = id,
            Email = "OLD@TEST.COM",
            PasswordHash = "old",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Users.Add(record);
        _dbContext.SaveChanges();
        _dbContext.Entry(record).State = EntityState.Detached; // Simulate starting from a domain object

        var user = User.Reconstitute(id, "NEW@TEST.COM", "new", true, record.CreatedAt, null);

        // Act
        _repository.Update(user);

        // Assert
        UserDbRecord? updatedRecord = _dbContext.Users.Local.FirstOrDefault(x => x.Id == id);
        updatedRecord.Should().NotBeNull();
        updatedRecord!.Email.Should().Be("NEW@TEST.COM");
        _dbContext.Entry(updatedRecord).State.Should().Be(EntityState.Modified);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
