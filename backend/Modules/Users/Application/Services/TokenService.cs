// <copyright file="TokenService.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Core.Application.Settings;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EleveRats.Modules.Users.Application.Services;

/// <summary>
/// Implementation of the token lifecycle management using JWT, Redis, and PostgreSQL.
/// Orchestrates access and refresh token creation, validation, and revocation
/// by delegating persistence concerns to the repository layer.
/// </summary>
internal sealed class TokenService(
    IOptions<JwtSettings> jwtSettings,
    ICacheService cacheService,
    IRefreshTokenRepository refreshTokenRepository,
    IUsersUnitOfWork unitOfWork
) : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUsersUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TokenResponse> GenerateTokenPairAsync(
        Guid userId,
        Guid profileId,
        Guid orgId,
        Guid? impersonatorId,
        string ipAddress
    )
    {
        string jti = Guid.NewGuid().ToString();
        string accessToken = GenerateJwt(userId, profileId, orgId, jti, impersonatorId);
        string rawRefreshToken = GenerateSecureRefreshToken();
        string tokenHash = HashToken(rawRefreshToken);

        // 1. Redis Trench (Access Token Insta-kill setup)
        string cacheKey = $"access_id:{jti}";
        var expiresIn = TimeSpan.FromMinutes(_jwtSettings.ExpirationMinutes);
        await _cacheService.SetAsync(cacheKey, "active", expiresIn);

        // 2. Postgres Trench (Refresh Token Persistence via domain entity)
        var refreshToken = RefreshToken.Create(
            userId: userId,
            tokenHash: tokenHash,
            expiresAt: DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            createdByIp: ipAddress
        );

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new TokenResponse(accessToken, rawRefreshToken, refreshToken.ExpiresAt, jti);
    }

    public async Task RevokeTokensAsync(Guid userId, string jti, string refreshToken)
    {
        // 1. Panic Button: Redis (Kills active JWT session instantly)
        string cacheKey = $"access_id:{jti}";
        await _cacheService.RemoveAsync(cacheKey);

        // 2. Panic Button: Postgres (Revokes refresh token to prevent rotation)
        string tokenHash = HashToken(refreshToken);
        RefreshToken? token = await _refreshTokenRepository.FindByUserAndHashAsync(
            userId,
            tokenHash
        );

        if (token is not null && token.IsActive)
        {
            token.Revoke();
            _refreshTokenRepository.Update(token);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generates a cryptographically strong random string for the refresh token.
    /// </summary>
    private static string GenerateSecureRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Hashes the refresh token before storing it in the database.
    /// SHA-256 is appropriate here because the token itself is 32 bytes of
    /// cryptographically random data (high entropy), making brute-force infeasible.
    /// </summary>
    private static string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Forges the actual JWT with the required business and security claims.
    /// </summary>
    private string GenerateJwt(
        Guid userId,
        Guid profileId,
        Guid orgId,
        string jti,
        Guid? impersonatorId
    )
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("profileId", profileId.ToString()),
            new("orgId", orgId.ToString()),
        };

        if (impersonatorId.HasValue)
        {
            claims.Add(new Claim("act", impersonatorId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
