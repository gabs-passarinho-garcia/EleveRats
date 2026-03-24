// <copyright file="OrganizationTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Organization"/> domain entity.
/// </summary>
public class OrganizationTests
{
    private const string ValidOrgName = "EleveRats Team";
    private const string ValidCreatedBy = "system_user";

    [Fact]
    public void Create_WithValidParameters_ShouldReturnActiveOrganization()
    {
        // Act
        var organization = Organization.Create(ValidOrgName, ValidCreatedBy);

        // Assert
        organization.Should().NotBeNull();
        organization.Id.Should().NotBe(Guid.Empty);
        organization.Name.Should().Be(ValidOrgName);
        organization.IsActive.Should().BeTrue();
        organization.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        organization.CreatedBy.Should().Be(ValidCreatedBy);
        organization.UpdatedAt.Should().BeNull();
        organization.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameWithSpaces_ShouldTrimName()
    {
        // Arrange
        var nameWithSpaces = "  Trimmed Org  ";

        // Act
        var organization = Organization.Create(nameWithSpaces, ValidCreatedBy);

        // Assert
        organization.Name.Should().Be("Trimmed Org");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("Ab")] // Too short
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        Action act = () => Organization.Create(invalidName!, ValidCreatedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCreatedBy_ShouldThrowArgumentException(string? invalidCreatedBy)
    {
        // Act
        Action act = () => Organization.Create(ValidOrgName, invalidCreatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Creator context*");
    }

    [Fact]
    public void Reconstitute_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;
        var updatedBy = "updater_user";

        // Act
        var organization = Organization.Reconstitute(
            id,
            ValidOrgName,
            false,
            createdAt,
            ValidCreatedBy,
            updatedAt,
            updatedBy
        );

        // Assert
        organization.Id.Should().Be(id);
        organization.Name.Should().Be(ValidOrgName);
        organization.IsActive.Should().BeFalse();
        organization.CreatedAt.Should().Be(createdAt);
        organization.CreatedBy.Should().Be(ValidCreatedBy);
        organization.UpdatedAt.Should().Be(updatedAt);
        organization.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndAuditInfo()
    {
        // Arrange
        var organization = Organization.Create(ValidOrgName, ValidCreatedBy);
        var newName = "New Org Name";
        var updatedBy = "renamer_user";

        // Act
        organization.Rename(newName, updatedBy);

        // Assert
        organization.Name.Should().Be(newName);
        organization.UpdatedBy.Should().Be(updatedBy);
        organization.UpdatedAt.Should().NotBeNull();
        organization
            .UpdatedAt.Value.Should()
            .BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalseAndSetTimestamp()
    {
        // Arrange
        var organization = Organization.Create(ValidOrgName, ValidCreatedBy);
        var updatedBy = "deactivator_user";

        // Act
        organization.Deactivate(updatedBy);

        // Assert
        organization.IsActive.Should().BeFalse();
        organization.UpdatedBy.Should().Be(updatedBy);
        organization.UpdatedAt.Should().NotBeNull();
        organization
            .UpdatedAt.Value.Should()
            .BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeIdempotent()
    {
        // Arrange
        var organization = Organization.Create(ValidOrgName, ValidCreatedBy);
        organization.Deactivate("first_user");
        var firstUpdate = organization.UpdatedAt;

        // Act
        organization.Deactivate("second_user");

        // Assert
        organization.IsActive.Should().BeFalse();
        organization.UpdatedAt.Should().Be(firstUpdate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Rename_WithEmptyUpdatedBy_ShouldThrowArgumentException(string? invalidUpdatedBy)
    {
        // Arrange
        var organization = Organization.Create(ValidOrgName, ValidCreatedBy);

        // Act
        Action act = () => organization.Rename("New Name", invalidUpdatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Updater context*");
    }
}
