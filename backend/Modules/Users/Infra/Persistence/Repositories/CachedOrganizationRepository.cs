// <copyright file="CachedOrganizationRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Decorator for IOrganizationRepository that adds distributed caching functionality.
/// Adheres to the Open/Closed Principle.
/// </summary>
internal sealed class CachedOrganizationRepository(
    IOrganizationRepository innerRepository,
    ICacheService cacheService,
    IOptions<UsersCacheOptions> options
) : IOrganizationRepository
{
    private const string _organizationCachePrefix = "EleveRats:Organization:";

    private readonly IOrganizationRepository _innerRepository =
        innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));

    private readonly ICacheService _cacheService =
        cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private readonly UsersCacheOptions _options =
        options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<Organization?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = $"{_organizationCachePrefix}{id}";

        Organization? cached = await _cacheService.GetAsync<Organization>(
            cacheKey,
            cancellationToken
        );
        if (cached is not null)
        {
            return cached;
        }

        Organization? organization = await _innerRepository.GetByIdAsync(id, cancellationToken);
        if (organization is not null)
        {
            await _cacheService.SetAsync(
                cacheKey,
                organization,
                TimeSpan.FromMinutes(_options.AbsoluteExpirationMinutes),
                cancellationToken
            );
        }

        return organization;
    }

    public Task AddAsync(
        Organization organization,
        CancellationToken cancellationToken = default
    ) => _innerRepository.AddAsync(organization, cancellationToken);

    public void Update(Organization organization)
    {
        _innerRepository.Update(organization);

        // Invalidate cache on update
        string cacheKey = $"{_organizationCachePrefix}{organization.Id}";
        _ = _cacheService.RemoveAsync(cacheKey);
    }
}
