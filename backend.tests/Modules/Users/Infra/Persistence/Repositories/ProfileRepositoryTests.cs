// <copyright file="ProfileRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Domain.Enums;
using EleveRats.Modules.Users.Infra.Persistence;
using EleveRats.Modules.Users.Infra.Persistence.Models;
using EleveRats.Modules.Users.Infra.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Infra.Persistence.Repositories;

/// <summary>
/// Unit tests for the <see cref="ProfileRepository"/>.
/// </summary>
public sealed class ProfileRepositoryTests : IDisposable
{
    private readonly UsersDbContext _dbContext;
    private readonly ProfileRepository _repository;

    public ProfileRepositoryTests()
    {
        DbContextOptions<UsersDbContext> options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new UsersDbContext(options);
        _repository = new ProfileRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProfileExists_ShouldReturnMappedDomainEntity()
    {
        // Arrange
        ProfileDbRecord record = new()
        {
            Id = Guid.CreateVersion7(),
            OrganizationId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            FullName = "Test Profile",
            BirthDate = new DateOnly(1990, 1, 1),
            Gender = Gender.Female,
            IsMember = true,
            ProfileType = ProfileType.Member,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
        };
        await _dbContext.Profiles.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        Profile? result = await _repository.GetByIdAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
        result.FullName.Should().Be(record.FullName);
        result.BirthDate.Should().Be(record.BirthDate);
    }

    [Fact]
    public async Task GetByUserIdAndOrganizationIdAsync_WhenProfileExists_ShouldReturnMappedDomainEntity()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();
        ProfileDbRecord record = new()
        {
            Id = Guid.CreateVersion7(),
            OrganizationId = orgId,
            UserId = userId,
            FullName = "Multi Key Profile",
            BirthDate = new DateOnly(1985, 5, 5),
            Gender = Gender.Male,
            IsMember = true,
            ProfileType = ProfileType.Admin,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
        };
        await _dbContext.Profiles.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        Profile? result = await _repository.GetByUserIdAndOrganizationIdAsync(userId, orgId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.OrganizationId.Should().Be(orgId);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRecordToDbContext()
    {
        // Arrange
        var profile = Profile.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "New Profile",
            new DateOnly(2000, 1, 1),
            Gender.Female,
            ProfileType.Member,
            "creator"
        );

        // Act
        await _repository.AddAsync(profile);

        // Assert
        ProfileDbRecord? record = _dbContext.Profiles.Local.FirstOrDefault(x => x.Id == profile.Id);
        record.Should().NotBeNull();
        record!.FullName.Should().Be(profile.FullName);
        record.BirthDate.Should().Be(profile.BirthDate);
    }

    [Fact]
    public void Update_ShouldChangeEntityStateToModified()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        ProfileDbRecord record = new()
        {
            Id = id,
            OrganizationId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            FullName = "Old Name",
            BirthDate = new DateOnly(1980, 1, 1),
            Gender = Gender.Male,
            IsMember = true,
            ProfileType = ProfileType.Member,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
        };
        _dbContext.Profiles.Add(record);
        _dbContext.SaveChanges();
        _dbContext.Entry(record).State = EntityState.Detached;

        var profile = Profile.Reconstitute(
            id,
            record.OrganizationId,
            record.UserId,
            "Updated Name",
            new DateOnly(1981, 10, 10),
            Gender.Female,
            true,
            ProfileType.Admin,
            record.CreatedAt,
            record.CreatedBy,
            DateTimeOffset.UtcNow,
            "updater"
        );

        // Act
        _repository.Update(profile);

        // Assert
        ProfileDbRecord? updatedRecord = _dbContext.Profiles.Local.FirstOrDefault(x => x.Id == id);
        updatedRecord.Should().NotBeNull();
        updatedRecord!.FullName.Should().Be("Updated Name");
        updatedRecord.BirthDate.Should().Be(new DateOnly(1981, 10, 10));
        _dbContext.Entry(updatedRecord).State.Should().Be(EntityState.Modified);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
