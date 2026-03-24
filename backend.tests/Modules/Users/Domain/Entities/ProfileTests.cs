// <copyright file="ProfileTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Profile"/> domain entity.
/// </summary>
public class ProfileTests
{
    private readonly Guid _organizationId = Guid.CreateVersion7();
    private readonly Guid _userId = Guid.CreateVersion7();
    private const string ValidFullName = "Gabriel Passarinho Garcia";
    private const int ValidAge = 28;
    private const Gender ValidGender = Gender.Male;
    private const ProfileType ValidProfileType = ProfileType.Admin;
    private const string ValidCreatedBy = "system_user";

    [Fact]
    public void Create_WithValidParameters_ShouldReturnProfile()
    {
        // Act
        var profile = Profile.Create(
            _organizationId,
            _userId,
            ValidFullName,
            ValidAge,
            ValidGender,
            ValidProfileType,
            ValidCreatedBy
        );

        // Assert
        profile.Should().NotBeNull();
        profile.Id.Should().NotBe(Guid.Empty);
        profile.OrganizationId.Should().Be(_organizationId);
        profile.UserId.Should().Be(_userId);
        profile.FullName.Should().Be(ValidFullName);
        profile.Age.Should().Be(ValidAge);
        profile.Gender.Should().Be(ValidGender);
        profile.ProfileType.Should().Be(ValidProfileType);
        profile.IsMember.Should().BeTrue();
        profile.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        profile.CreatedBy.Should().Be(ValidCreatedBy);
        profile.UpdatedAt.Should().BeNull();
        profile.UpdatedBy.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("Ab")] // Too short
    public void Create_WithInvalidFullName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        Action act = () =>
            Profile.Create(
                _organizationId,
                _userId,
                invalidName,
                ValidAge,
                ValidGender,
                ValidProfileType,
                ValidCreatedBy
            );

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*full name*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(131)]
    public void Create_WithInvalidAge_ShouldThrowArgumentException(int invalidAge)
    {
        // Act
        Action act = () =>
            Profile.Create(
                _organizationId,
                _userId,
                ValidFullName,
                invalidAge,
                ValidGender,
                ValidProfileType,
                ValidCreatedBy
            );

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Age*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCreatedBy_ShouldThrowArgumentException(string? invalidCreatedBy)
    {
        // Act
        Action act = () =>
            Profile.Create(
                _organizationId,
                _userId,
                ValidFullName,
                ValidAge,
                ValidGender,
                ValidProfileType,
                invalidCreatedBy
            );

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Creator context*");
    }

    [Fact]
    public void Reconstitute_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-1);
        var updatedAt = DateTimeOffset.UtcNow;
        var updatedBy = "updater_user";

        // Act
        var profile = Profile.Reconstitute(
            id,
            _organizationId,
            _userId,
            ValidFullName,
            ValidAge,
            ValidGender,
            false,
            ProfileType.Member,
            createdAt,
            ValidCreatedBy,
            updatedAt,
            updatedBy
        );

        // Assert
        profile.Id.Should().Be(id);
        profile.OrganizationId.Should().Be(_organizationId);
        profile.UserId.Should().Be(_userId);
        profile.FullName.Should().Be(ValidFullName);
        profile.Age.Should().Be(ValidAge);
        profile.Gender.Should().Be(ValidGender);
        profile.IsMember.Should().BeFalse();
        profile.ProfileType.Should().Be(ProfileType.Member);
        profile.CreatedAt.Should().Be(createdAt);
        profile.CreatedBy.Should().Be(ValidCreatedBy);
        profile.UpdatedAt.Should().Be(updatedAt);
        profile.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void UpdateDetails_WithValidParameters_ShouldUpdateFieldsAndAuditInfo()
    {
        // Arrange
        var profile = Profile.Create(
            _organizationId,
            _userId,
            ValidFullName,
            ValidAge,
            ValidGender,
            ValidProfileType,
            ValidCreatedBy
        );

        var newName = "New Name Here  "; // With trailing spaces
        var newAge = 30;
        var newGender = Gender.Female;
        var updatedBy = "new_updater";

        // Act
        profile.UpdateDetails(newName, newAge, newGender, updatedBy);

        // Assert
        profile.FullName.Should().Be(newName.Trim());
        profile.Age.Should().Be(newAge);
        profile.Gender.Should().Be(newGender);
        profile.UpdatedBy.Should().Be(updatedBy);
        profile.UpdatedAt.Should().NotBeNull();
        profile.UpdatedAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ChangeAccessLevel_ShouldUpdateProfileTypeAndAuditInfo()
    {
        // Arrange
        var profile = Profile.Create(
            _organizationId,
            _userId,
            ValidFullName,
            ValidAge,
            ValidGender,
            ProfileType.Member,
            ValidCreatedBy
        );

        var updatedBy = "admin_updater";

        // Act
        profile.ChangeAccessLevel(ProfileType.Admin, updatedBy);

        // Assert
        profile.ProfileType.Should().Be(ProfileType.Admin);
        profile.UpdatedBy.Should().Be(updatedBy);
        profile.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RevokeMembership_ShouldSetIsMemberToFalseAndAuditInfo()
    {
        // Arrange
        var profile = Profile.Create(
            _organizationId,
            _userId,
            ValidFullName,
            ValidAge,
            ValidGender,
            ProfileType.Member,
            ValidCreatedBy
        );

        var updatedBy = "security_officer";

        // Act
        profile.RevokeMembership(updatedBy);

        // Assert
        profile.IsMember.Should().BeFalse();
        profile.UpdatedBy.Should().Be(updatedBy);
        profile.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateDetails_WithEmptyUpdatedBy_ShouldThrowArgumentException(
        string? invalidUpdatedBy
    )
    {
        // Arrange
        var profile = Profile.Create(
            _organizationId,
            _userId,
            ValidFullName,
            ValidAge,
            ValidGender,
            ValidProfileType,
            ValidCreatedBy
        );

        // Act
        Action act = () => profile.UpdateDetails("New Name", 20, Gender.Female, invalidUpdatedBy);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Updater context*");
    }
}
