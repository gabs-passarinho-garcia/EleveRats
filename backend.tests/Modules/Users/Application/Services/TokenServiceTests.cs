// <copyright file="TokenServiceTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Core.Application.Settings;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Application.Services;
using EleveRats.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Application.Services;

/// <summary>
/// Unit tests for the <see cref="TokenService"/>.
/// Dependencies are mocked with NSubstitute to isolate business logic.
/// </summary>
public class TokenServiceTests
{
    private readonly ICacheService _cacheService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUsersUnitOfWork _unitOfWork;
    private readonly TokenService _sut;

    private static readonly JwtSettings _jwtSettings = new()
    {
        SecretKey = "this-is-a-super-secret-key-for-testing-that-is-long-enough",
        Issuer = "EleveRats.Tests",
        Audience = "EleveRats.Tests",
        ExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7,
    };

    public TokenServiceTests()
    {
        _cacheService = Substitute.For<ICacheService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUsersUnitOfWork>();

        IOptions<JwtSettings> options = Options.Create(_jwtSettings);

        _sut = new TokenService(options, _cacheService, _refreshTokenRepository, _unitOfWork);
    }

    // -----------------------------------------------------------------------
    // GenerateTokenPairAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateTokenPairAsync_ShouldReturnValidTokenResponse()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var profileId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();
        string ipAddress = "127.0.0.1";

        // Act
        TokenResponse result = await _sut.GenerateTokenPairAsync(
            userId,
            profileId,
            orgId,
            null,
            ipAddress
        );

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.AccessId.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateTokenPairAsync_ShouldGenerateValidJwtWithCorrectClaims()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var profileId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();

        // Act
        TokenResponse result = await _sut.GenerateTokenPairAsync(
            userId,
            profileId,
            orgId,
            null,
            "10.0.0.1"
        );

        // Assert — decode the JWT and verify claims without a real validator
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt = handler.ReadJwtToken(result.AccessToken);

        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "profileId" && c.Value == profileId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "orgId" && c.Value == orgId.ToString());
        jwt.Id.Should().Be(result.AccessId);
    }

    [Fact]
    public async Task GenerateTokenPairAsync_WithImpersonatorId_ShouldIncludeActClaim()
    {
        // Arrange
        var impersonatorId = Guid.CreateVersion7();

        // Act
        TokenResponse result = await _sut.GenerateTokenPairAsync(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            impersonatorId,
            "10.0.0.1"
        );

        // Assert
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt = handler.ReadJwtToken(result.AccessToken);
        jwt.Claims.Should().Contain(c => c.Type == "act" && c.Value == impersonatorId.ToString());
    }

    [Fact]
    public async Task GenerateTokenPairAsync_WithoutImpersonatorId_ShouldNotIncludeActClaim()
    {
        // Act
        TokenResponse result = await _sut.GenerateTokenPairAsync(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            impersonatorId: null,
            "10.0.0.1"
        );

        // Assert
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt = handler.ReadJwtToken(result.AccessToken);
        jwt.Claims.Should().NotContain(c => c.Type == "act");
    }

    [Fact]
    public async Task GenerateTokenPairAsync_ShouldCallCacheServiceWithJtiKeyAndTtl()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        // Act
        TokenResponse result = await _sut.GenerateTokenPairAsync(
            userId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            null,
            "10.0.0.1"
        );

        // Assert — verify the cache was called with the expected key format and TTL
        string expectedKey = $"access_id:{result.AccessId}";
        await _cacheService
            .Received(1)
            .SetAsync(expectedKey, "active", TimeSpan.FromMinutes(_jwtSettings.ExpirationMinutes));
    }

    [Fact]
    public async Task GenerateTokenPairAsync_ShouldAddTokenToRepositoryAndCommit()
    {
        // Act
        await _sut.GenerateTokenPairAsync(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            null,
            "10.0.0.1"
        );

        // Assert — only 1 token added and 1 UoW commit
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // RevokeTokensAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RevokeTokensAsync_ShouldRemoveCacheEntryForJti()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        string jti = Guid.NewGuid().ToString();
        string rawRefreshToken = "any_raw_token";

        _refreshTokenRepository
            .FindByUserAndHashAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ReturnsNull();

        // Act
        await _sut.RevokeTokensAsync(userId, jti, rawRefreshToken);

        // Assert
        await _cacheService.Received(1).RemoveAsync($"access_id:{jti}");
    }

    [Fact]
    public async Task RevokeTokensAsync_WhenActiveTokenIsFound_ShouldRevokeAndPersist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var activeToken = RefreshToken.Create(
            userId,
            "any_hash",
            DateTime.UtcNow.AddDays(7),
            "1.1.1.1"
        );

        _refreshTokenRepository
            .FindByUserAndHashAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(activeToken);

        // Act
        await _sut.RevokeTokensAsync(userId, Guid.NewGuid().ToString(), "raw_token");

        // Assert
        activeToken.RevokedAt.Should().NotBeNull();
        activeToken.IsActive.Should().BeFalse();
        _refreshTokenRepository.Received(1).Update(activeToken);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RevokeTokensAsync_WhenTokenIsNotFound_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        _refreshTokenRepository
            .FindByUserAndHashAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ReturnsNull();

        // Act
        await _sut.RevokeTokensAsync(Guid.CreateVersion7(), Guid.NewGuid().ToString(), "raw_token");

        // Assert
        _refreshTokenRepository.DidNotReceive().Update(Arg.Any<RefreshToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task RevokeTokensAsync_WhenTokenIsAlreadyRevoked_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        var revokedToken = RefreshToken.Reconstitute(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "any_hash",
            DateTime.UtcNow.AddDays(7),
            "1.1.1.1",
            revokedAt: DateTime.UtcNow.AddHours(-1)
        );

        _refreshTokenRepository
            .FindByUserAndHashAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(revokedToken);

        // Act
        await _sut.RevokeTokensAsync(Guid.CreateVersion7(), Guid.NewGuid().ToString(), "raw_token");

        // Assert — already revoked, no update needed
        _refreshTokenRepository.DidNotReceive().Update(Arg.Any<RefreshToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
