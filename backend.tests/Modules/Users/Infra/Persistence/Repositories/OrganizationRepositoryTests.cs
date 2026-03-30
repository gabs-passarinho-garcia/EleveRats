// <copyright file="OrganizationRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Unit tests for the <see cref="OrganizationRepository"/>.
/// </summary>
public sealed class OrganizationRepositoryTests : IDisposable
{
    private readonly UsersDbContext _dbContext;
    private readonly OrganizationRepository _repository;

    public OrganizationRepositoryTests()
    {
        DbContextOptions<UsersDbContext> options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new UsersDbContext(options);
        _repository = new OrganizationRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganizationExists_ShouldReturnMappedDomainEntity()
    {
        // Arrange
        var record = new OrganizationDbRecord
        {
            Id = Guid.CreateVersion7(),
            Name = "Test Org",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
        };
        await _dbContext.Organizations.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        Organization? result = await _repository.GetByIdAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
        result.Name.Should().Be(record.Name);
        result.IsActive.Should().BeTrue(); // Logic: defined as true if loaded and not deleted
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganizationIsDeleted_ShouldReturnNullDueToGlobalFilter()
    {
        // Arrange
        var record = new OrganizationDbRecord
        {
            Id = Guid.CreateVersion7(),
            Name = "Deleted Org",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            DeletedAt = DateTimeOffset.UtcNow, // Soft deleted
            DeletedBy = "admin",
        };
        await _dbContext.Organizations.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        // Act
        Organization? result = await _repository.GetByIdAsync(record.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddRecordToDbContext()
    {
        // Arrange
        var org = Organization.Create("New Org", "creator");

        // Act
        await _repository.AddAsync(org);

        // Assert
        OrganizationDbRecord? record = _dbContext.Organizations.Local.FirstOrDefault(x =>
            x.Id == org.Id
        );
        record.Should().NotBeNull();
        record!.Name.Should().Be(org.Name);
    }

    [Fact]
    public void Update_ShouldChangeEntityStateToModified()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var record = new OrganizationDbRecord
        {
            Id = id,
            Name = "Old Name",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
        };
        _dbContext.Organizations.Add(record);
        _dbContext.SaveChanges();
        _dbContext.Entry(record).State = EntityState.Detached;

        var org = Organization.Reconstitute(
            id,
            "New Name",
            true,
            record.CreatedAt,
            record.CreatedBy,
            null,
            null
        );

        // Act
        _repository.Update(org);

        // Assert
        OrganizationDbRecord? updatedRecord = _dbContext.Organizations.Local.FirstOrDefault(x =>
            x.Id == id
        );
        updatedRecord.Should().NotBeNull();
        updatedRecord!.Name.Should().Be("New Name");
        _dbContext.Entry(updatedRecord).State.Should().Be(EntityState.Modified);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
