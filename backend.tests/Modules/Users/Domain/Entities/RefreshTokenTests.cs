// <copyright file="RefreshTokenTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using AwesomeAssertions;
using EleveRats.Modules.Users.Domain.Entities;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="RefreshToken"/> domain entity.
/// Covers factory methods, computed properties, and domain behaviors.
/// </summary>
public class RefreshTokenTests
{
    private static readonly Guid _userId = Guid.CreateVersion7();
    private const string _validHash = "hashed_token_value";
    private const string _validIp = "192.168.0.1";

    // -----------------------------------------------------------------------
    // Create
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WithValidParameters_ShouldReturnActiveToken()
    {
        // Arrange
        DateTime expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = RefreshToken.Create(_userId, _validHash, expiresAt, _validIp);

        // Assert
        token.Should().NotBeNull();
        token.Id.Should().NotBe(Guid.Empty);
        token.UserId.Should().Be(_userId);
        token.TokenHash.Should().Be(_validHash);
        token.ExpiresAt.Should().Be(expiresAt);
        token.CreatedByIp.Should().Be(_validIp);
        token.RevokedAt.Should().BeNull();
        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullTokenHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Act
        Action act = () =>
            RefreshToken.Create(_userId, invalidHash!, DateTime.UtcNow.AddDays(1), _validIp);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*hash*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullIpAddress_ShouldThrowArgumentException(string? invalidIp)
    {
        // Act
        Action act = () =>
            RefreshToken.Create(_userId, _validHash, DateTime.UtcNow.AddDays(1), invalidIp!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*IP*");
    }

    [Fact]
    public void Create_WithPastExpirationDate_ShouldThrowArgumentException()
    {
        // Arrange
        DateTime pastDate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        Action act = () => RefreshToken.Create(_userId, _validHash, pastDate, _validIp);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Expiration*");
    }

    // -----------------------------------------------------------------------
    // Reconstitute
    // -----------------------------------------------------------------------

    [Fact]
    public void Reconstitute_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        DateTime expiresAt = DateTime.UtcNow.AddDays(7);
        DateTime revokedAt = DateTime.UtcNow.AddDays(1);

        // Act
        var token = RefreshToken.Reconstitute(
            id,
            _userId,
            _validHash,
            expiresAt,
            _validIp,
            revokedAt
        );

        // Assert
        token.Id.Should().Be(id);
        token.UserId.Should().Be(_userId);
        token.TokenHash.Should().Be(_validHash);
        token.ExpiresAt.Should().Be(expiresAt);
        token.CreatedByIp.Should().Be(_validIp);
        token.RevokedAt.Should().Be(revokedAt);
    }

    [Fact]
    public void Reconstitute_WithNullRevokedAt_ShouldProduceActiveToken()
    {
        // Act
        var token = RefreshToken.Reconstitute(
            Guid.CreateVersion7(),
            _userId,
            _validHash,
            DateTime.UtcNow.AddDays(7),
            _validIp,
            revokedAt: null
        );

        // Assert
        token.RevokedAt.Should().BeNull();
        token.IsActive.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // IsActive / IsExpired computed properties
    // -----------------------------------------------------------------------

    [Fact]
    public void IsExpired_WhenExpirationIsInThePast_ShouldBeTrue()
    {
        // Arrange
        var token = RefreshToken.Reconstitute(
            Guid.CreateVersion7(),
            _userId,
            _validHash,
            expiresAt: DateTime.UtcNow.AddSeconds(-1),
            _validIp,
            revokedAt: null
        );

        // Assert
        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenTokenIsRevokedAndNotExpired_ShouldBeFalse()
    {
        // Arrange
        var token = RefreshToken.Reconstitute(
            Guid.CreateVersion7(),
            _userId,
            _validHash,
            expiresAt: DateTime.UtcNow.AddDays(7),
            _validIp,
            revokedAt: DateTime.UtcNow
        );

        // Assert
        token.IsActive.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Revoke
    // -----------------------------------------------------------------------

    [Fact]
    public void Revoke_WhenTokenIsActive_ShouldSetRevokedAtAndDeactivate()
    {
        // Arrange
        var token = RefreshToken.Create(_userId, _validHash, DateTime.UtcNow.AddDays(7), _validIp);

        // Act
        token.Revoke();

        // Assert
        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WhenTokenIsAlreadyRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var token = RefreshToken.Create(_userId, _validHash, DateTime.UtcNow.AddDays(7), _validIp);
        token.Revoke();

        // Act
        Action act = () => token.Revoke();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*already been revoked*");
    }
}
