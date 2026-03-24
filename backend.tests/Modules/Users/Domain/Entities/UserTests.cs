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
using EleveRats.Modules.Users.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="User"/> domain entity.
/// </summary>
public class UserTests
{
    private const string ValidEmail = "test@eleverats.com";
    private const string ValidPasswordHash = "hashed_password";

    [Fact]
    public void Create_WithValidParameters_ShouldReturnActiveUser()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(ValidEmail.ToUpperInvariant());
        user.PasswordHash.Should().Be(ValidPasswordHash);
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
        Action act = () => User.Create(invalidEmail, ValidPasswordHash);

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
        Action act = () => User.Create(ValidEmail, invalidHash);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*password hash*");
    }

    [Fact]
    public void Reconstitute_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var email = "persisted@test.com";
        var hash = "old_hash";
        var isActive = false;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

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
        var user = User.Create(ValidEmail, ValidPasswordHash);
        var newHash = "new_secure_hash";

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
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Act
        Action act = () => user.ChangePassword(invalidHash);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*password hash*");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalseAndSetTimestamp()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);

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
        var user = User.Create(ValidEmail, ValidPasswordHash);
        user.Deactivate();
        var firstUpdate = user.UpdatedAt;

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().Be(firstUpdate);
    }
}
