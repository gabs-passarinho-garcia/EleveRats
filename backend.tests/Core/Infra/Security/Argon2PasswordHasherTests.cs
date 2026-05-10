// <copyright file="Argon2PasswordHasherTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using AwesomeAssertions;
using EleveRats.Core.Infra.Security;
using Xunit;

namespace EleveRats.Tests.Core.Infra.Security;

/// <summary>
/// Unit tests for the Argon2PasswordHasher class.
/// </summary>
public class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _hasher;

    public Argon2PasswordHasherTests() => _hasher = new Argon2PasswordHasher();

    [Fact]
    public void HashPassword_ShouldReturnValidHashString()
    {
        // Arrange
        string password = "StrongPassword123!";

        // Act
        string hash = _hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$argon2id$", "because the output should be in PHC format");

        string[] parts = hash.Split('$');
        parts.Length.Should().Be(6); // Empty, argon2id, v=19, parameters, salt, hash

        // Ensure they are valid Base64
        Action actSalt = () => Convert.FromBase64String(parts[4]);
        Action actHash = () => Convert.FromBase64String(parts[5]);

        actSalt.Should().NotThrow();
        actHash.Should().NotThrow();
    }

    [Fact]
    public void VerifyPassword_WithLegacyHash_ShouldReturnFalseForInvalidHash()
    {
        // Arrange
        string password = "LegacyPassword";
        string malformedLegacyHash = "invalid-salt:invalid-hash";

        // Act
        bool result = _hasher.VerifyPassword(password, malformedLegacyHash);

        // Assert
        result.Should().BeFalse("because the legacy hash provided is invalid");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        string password = "SecretPassword";
        string hash = _hasher.HashPassword(password);

        // Act
        bool result = _hasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue("because the correct password was provided");
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        string password = "SecretPassword";
        string wrongPassword = "WrongPassword";
        string hash = _hasher.HashPassword(password);

        // Act
        bool result = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse("because the password provided was incorrect");
    }

    [Fact]
    public void VerifyPassword_WithMalformedHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "SecretPassword";
        string malformedHash = "ThisIsnotAValidHashString";

        // Act
        bool result = _hasher.VerifyPassword(password, malformedHash);

        // Assert
        result.Should().BeFalse("because the hash string is malformed");
    }

    [Theory]
    [InlineData(null, "some-hash")]
    [InlineData("", "some-hash")]
    [InlineData("   ", "some-hash")]
    [InlineData("password", null)]
    [InlineData("password", "")]
    [InlineData("password", "   ")]
    public void VerifyPassword_WithInvalidInputs_ShouldThrowArgumentNullException(
        string? password,
        string? hash
    )
    {
        // Act
        Action act = () => _hasher.VerifyPassword(password!, hash!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForSamePassword()
    {
        // Arrange
        string password = "EqualPassword";

        // Act
        string hash1 = _hasher.HashPassword(password);
        string hash2 = _hasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "because each hash should have a unique salt");
    }
}
