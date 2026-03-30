// <copyright file="RedisCacheService.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EleveRats.Core.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EleveRats.Core.Infra.Caching;

/// <summary>
/// Redis-backed implementation of the ICacheService.
/// Uses Microsoft.Extensions.Caching.Distributed for distributed caching
/// and System.Text.Json for object serialization.
/// </summary>
internal sealed class RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly Action<ILogger, string, Exception?> _logRetrievalError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(GetAsync)),
            "Failed to retrieve key '{Key}' from Redis cache."
        );

    private static readonly Action<ILogger, string, Exception?> _logSetError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(SetAsync)),
            "Failed to set key '{Key}' in Redis cache."
        );

    private static readonly Action<ILogger, string, Exception?> _logRemoveError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, nameof(RemoveAsync)),
            "Failed to remove key '{Key}' from Redis cache."
        );

    private readonly IDistributedCache _cache =
        cache ?? throw new ArgumentNullException(nameof(cache));

    private readonly ILogger<RedisCacheService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            string? cachedValue = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrWhiteSpace(cachedValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logRetrievalError(_logger, key, ex);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default
    )
    {
        if (value is null)
        {
            return;
        }

        try
        {
            var options = new DistributedCacheEntryOptions();

            if (absoluteExpiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
            }

            string serializedValue = JsonSerializer.Serialize(value);

            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logSetError(_logger, key, ex);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logRemoveError(_logger, key, ex);
        }
    }
}
