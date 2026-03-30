// <copyright file="CachedUserRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Decorator for IUserRepository that adds distributed caching functionality.
/// Adheres to the Open/Closed Principle by adding behavior without modifying the original implementation.
/// </summary>
internal sealed class CachedUserRepository(
    IUserRepository innerRepository,
    ICacheService cacheService,
    IOptions<UsersCacheOptions> options
) : IUserRepository
{
    private const string _userCachePrefix = "EleveRats:User:";

    private readonly IUserRepository _innerRepository =
        innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));

    private readonly ICacheService _cacheService =
        cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private readonly UsersCacheOptions _options =
        options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"{_userCachePrefix}{id}";

        // 1. Try to fetch from cache
        User? cachedUser = await _cacheService.GetAsync<User>(cacheKey, cancellationToken);

        if (cachedUser is not null)
        {
            return cachedUser;
        }

        // 2. Cache miss: Fetch from database
        User? user = await _innerRepository.GetByIdAsync(id, cancellationToken);

        if (user is not null)
        {
            // 3. Store in cache for future requests
            await _cacheService.SetAsync(
                cacheKey,
                user,
                TimeSpan.FromMinutes(_options.AbsoluteExpirationMinutes),
                cancellationToken
            );
        }

        return user;
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    ) => _innerRepository.GetByEmailAsync(email, cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        _innerRepository.AddAsync(user, cancellationToken);

    public void Update(User user)
    {
        _innerRepository.Update(user);

        // Invalidate cache on update to maintain consistency
        string cacheKey = $"{_userCachePrefix}{user.Id}";
        _ = _cacheService.RemoveAsync(cacheKey);
    }
}
