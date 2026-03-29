// <copyright file="UserTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using AwesomeAssertions;
using EleveRats.Modules.Users.Domain.Entities;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="User"/> domain entity.
/// </summary>
public class UserTests
{
    private const string _validEmail = "test@eleverats.com";
    private const string _validPasswordHash = "hashed_password";

    [Fact]
    public void Create_WithValidParameters_ShouldReturnActiveUser()
    {
        // Act
        var user = User.Create(_validEmail, _validPasswordHash);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(_validEmail.ToUpperInvariant());
        user.PasswordHash.Should().Be(_validPasswordHash);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@eleverats.com")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        Action act = () => User.Create(invalidEmail!, _validPasswordHash);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidPasswordHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Act
        Action act = () => User.Create(_validEmail, invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*password hash*");
    }

    [Fact]
    public void Reconstitute_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        string email = "persisted@test.com";
        string hash = "old_hash";
        bool isActive = false;
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;

        // Act
        var user = User.Reconstitute(id, email, hash, isActive, createdAt, updatedAt);

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(hash);
        user.IsActive.Should().Be(isActive);
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdatePasswordAndTimestamp()
    {
        // Arrange
        var user = User.Create(_validEmail, _validPasswordHash);
        string newHash = "new_secure_hash";

        // Act
        user.ChangePassword(newHash);

        // Assert
        user.PasswordHash.Should().Be(newHash);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangePassword_WithInvalidHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Arrange
        var user = User.Create(_validEmail, _validPasswordHash);

        // Act
        Action act = () => user.ChangePassword(invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*password hash*");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalseAndSetTimestamp()
    {
        // Arrange
        var user = User.Create(_validEmail, _validPasswordHash);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeIdempotent()
    {
        // Arrange
        var user = User.Create(_validEmail, _validPasswordHash);
        user.Deactivate();
        DateTimeOffset? firstUpdate = user.UpdatedAt;

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().Be(firstUpdate);
    }
}
