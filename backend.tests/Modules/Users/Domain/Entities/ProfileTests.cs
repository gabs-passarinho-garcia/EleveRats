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
using AwesomeAssertions;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Profile"/> domain entity.
/// </summary>
public class ProfileTests
{
    private readonly Guid _organizationId = Guid.CreateVersion7();
    private readonly Guid _userId = Guid.CreateVersion7();
    private const string _validFullName = "Gabriel Passarinho Garcia";
    private const int _validAge = 28;
    private const Gender _validGender = Gender.Male;
    private const ProfileType _validProfileType = ProfileType.Admin;
    private const string _validCreatedBy = "system_user";

    [Fact]
    public void Create_WithValidParameters_ShouldReturnProfile()
    {
        // Act
        var profile = Profile.Create(
            _organizationId,
            _userId,
            _validFullName,
            _validAge,
            _validGender,
            _validProfileType,
            _validCreatedBy
        );

        // Assert
        profile.Should().NotBeNull();
        profile.Id.Should().NotBe(Guid.Empty);
        profile.OrganizationId.Should().Be(_organizationId);
        profile.UserId.Should().Be(_userId);
        profile.FullName.Should().Be(_validFullName);
        profile.Age.Should().Be(_validAge);
        profile.Gender.Should().Be(_validGender);
        profile.ProfileType.Should().Be(_validProfileType);
        profile.IsMember.Should().BeTrue();
        profile.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        profile.CreatedBy.Should().Be(_validCreatedBy);
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
                invalidName!,
                _validAge,
                _validGender,
                _validProfileType,
                _validCreatedBy
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
                _validFullName,
                invalidAge,
                _validGender,
                _validProfileType,
                _validCreatedBy
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
                _validFullName,
                _validAge,
                _validGender,
                _validProfileType,
                invalidCreatedBy!
            );

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Creator context*");
    }

    [Fact]
    public void Reconstitute_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddHours(-1);
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
        string updatedBy = "updater_user";

        // Act
        var profile = Profile.Reconstitute(
            id,
            _organizationId,
            _userId,
            _validFullName,
            _validAge,
            _validGender,
            false,
            ProfileType.Member,
            createdAt,
            _validCreatedBy,
            updatedAt,
            updatedBy
        );

        // Assert
        profile.Id.Should().Be(id);
        profile.OrganizationId.Should().Be(_organizationId);
        profile.UserId.Should().Be(_userId);
        profile.FullName.Should().Be(_validFullName);
        profile.Age.Should().Be(_validAge);
        profile.Gender.Should().Be(_validGender);
        profile.IsMember.Should().BeFalse();
        profile.ProfileType.Should().Be(ProfileType.Member);
        profile.CreatedAt.Should().Be(createdAt);
        profile.CreatedBy.Should().Be(_validCreatedBy);
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
            _validFullName,
            _validAge,
            _validGender,
            _validProfileType,
            _validCreatedBy
        );

        string newName = "New Name Here  "; // With trailing spaces
        int newAge = 30;
        Gender newGender = Gender.Female;
        string updatedBy = "new_updater";

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
            _validFullName,
            _validAge,
            _validGender,
            ProfileType.Member,
            _validCreatedBy
        );

        string updatedBy = "admin_updater";

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
            _validFullName,
            _validAge,
            _validGender,
            ProfileType.Member,
            _validCreatedBy
        );

        string updatedBy = "security_officer";

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
            _validFullName,
            _validAge,
            _validGender,
            _validProfileType,
            _validCreatedBy
        );

        // Act
        Action act = () => profile.UpdateDetails("New Name", 20, Gender.Female, invalidUpdatedBy!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Updater context*");
    }
}
