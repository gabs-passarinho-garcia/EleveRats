// <copyright file="CachedProfileRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Threading;
using System.Threading.Tasks;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Options;

namespace EleveRats.Modules.Users.Infra.Persistence.Repositories;

/// <summary>
/// Decorator for IProfileRepository that adds distributed caching functionality.
/// Adheres to the Open/Closed Principle.
/// </summary>
internal sealed class CachedProfileRepository(
    IProfileRepository innerRepository,
    ICacheService cacheService,
    IOptions<UsersCacheOptions> options
) : IProfileRepository
{
    private const string _profileCachePrefix = "EleveRats:Profile:";
    private const string _profileUserOrgCachePrefix = "EleveRats:Profile:UserOrg:";

    private readonly IProfileRepository _innerRepository =
        innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));

    private readonly ICacheService _cacheService =
        cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private readonly UsersCacheOptions _options =
        options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"{_profileCachePrefix}{id}";

        Profile? cached = await _cacheService.GetAsync<Profile>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        Profile? profile = await _innerRepository.GetByIdAsync(id, cancellationToken);
        if (profile is not null)
        {
            await CacheProfileAsync(profile, cancellationToken);
        }

        return profile;
    }

    public async Task<Profile?> GetByUserIdAndOrganizationIdAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = $"{_profileUserOrgCachePrefix}{userId}:{organizationId}";

        Profile? cached = await _cacheService.GetAsync<Profile>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        Profile? profile = await _innerRepository.GetByUserIdAndOrganizationIdAsync(
            userId,
            organizationId,
            cancellationToken
        );

        if (profile is not null)
        {
            await CacheProfileAsync(profile, cancellationToken);
        }

        return profile;
    }

    public Task AddAsync(Profile profile, CancellationToken cancellationToken = default) =>
        _innerRepository.AddAsync(profile, cancellationToken);

    public void Update(Profile profile)
    {
        _innerRepository.Update(profile);

        // Invalidate both cache entries to maintain consistency
        string keyById = $"{_profileCachePrefix}{profile.Id}";
        string keyByUserOrg =
            $"{_profileUserOrgCachePrefix}{profile.UserId}:{profile.OrganizationId}";

        _ = _cacheService.RemoveAsync(keyById);
        _ = _cacheService.RemoveAsync(keyByUserOrg);
    }

    private async Task CacheProfileAsync(Profile profile, CancellationToken cancellationToken)
    {
        string keyById = $"{_profileCachePrefix}{profile.Id}";
        string keyByUserOrg =
            $"{_profileUserOrgCachePrefix}{profile.UserId}:{profile.OrganizationId}";
        var expiration = TimeSpan.FromMinutes(_options.AbsoluteExpirationMinutes);

        await _cacheService.SetAsync(keyById, profile, expiration, cancellationToken);
        await _cacheService.SetAsync(keyByUserOrg, profile, expiration, cancellationToken);
    }
}
