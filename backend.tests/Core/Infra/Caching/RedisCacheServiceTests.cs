// <copyright file="RedisCacheServiceTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Core.Infra.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EleveRats.Tests.Core.Infra.Caching;

public sealed class RedisCacheServiceTests
{
    private readonly IDistributedCache _distributedCache = Substitute.For<IDistributedCache>();
    private readonly ILogger<RedisCacheService> _logger = Substitute.For<
        ILogger<RedisCacheService>
    >();
    private readonly RedisCacheService _service;

    public RedisCacheServiceTests() => _service = new RedisCacheService(_distributedCache, _logger);

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedValue()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = new TestObject { Id = 1, Name = "Test" };
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(expectedValue);
        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns(data);

        // Act
        TestObject? result = await _service.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedValue.Id);
        result.Name.Should().Be(expectedValue.Name);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault()
    {
        // Arrange
        _distributedCache
            .GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        TestObject? result = await _service.GetAsync<TestObject>("missing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenExceptionOccurs_ReturnsDefaultAndLogsError()
    {
        // Arrange
        _distributedCache
            .GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Redis down"));

        // Act
        TestObject? result = await _service.GetAsync<TestObject>("any-key");

        // Assert
        result.Should().BeNull();
        // Verify logger was called
        _ = _logger
            .ReceivedCalls()
            .Should()
            .ContainSingle(static c => IsErrorLogCall(c));
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndStoreValue()
    {
        // Arrange
        string key = "set-key";
        var value = new TestObject { Id = 2, Name = "SetTest" };

        // Act
        await _service.SetAsync(key, value, TimeSpan.FromMinutes(10));

        // Assert
        await _distributedCache
            .Received(1)
            .SetAsync(
                key,
                Arg.Is<byte[]>(b => JsonSerializer.Deserialize<TestObject>(b)!.Name == value.Name),
                Arg.Is<DistributedCacheEntryOptions>(o =>
                    o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(10)
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallCacheRemove()
    {
        // Arrange
        string key = "remove-key";

        // Act
        await _service.RemoveAsync(key);

        // Assert
        await _distributedCache.Received(1).RemoveAsync(key, Arg.Any<CancellationToken>());
    }

    private sealed class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private static bool IsErrorLogCall(ICall call)
    {
        object?[] arguments = call.GetArguments();
        if (arguments.Length == 0)
        {
            return false;
        }

        return arguments[0] is LogLevel level && level == LogLevel.Error;
    }
}
