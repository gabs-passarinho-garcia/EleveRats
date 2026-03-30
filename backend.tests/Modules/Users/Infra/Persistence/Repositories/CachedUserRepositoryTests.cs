// <copyright file="CachedUserRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using AwesomeAssertions;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Modules.Users;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Domain.Entities;
using EleveRats.Modules.Users.Infra.Persistence.Repositories;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Infra.Persistence.Repositories;

public sealed class CachedUserRepositoryTests
{
    private readonly IUserRepository _innerRepository = Substitute.For<IUserRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IOptions<UsersCacheOptions> _options = Substitute.For<
        IOptions<UsersCacheOptions>
    >();
    private readonly CachedUserRepository _decorator;

    public CachedUserRepositoryTests()
    {
        _options.Value.Returns(new UsersCacheOptions { AbsoluteExpirationMinutes = 5 });
        _decorator = new CachedUserRepository(_innerRepository, _cacheService, _options);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheHit_ShouldReturnCachedUserAndNotCallInnerRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cachedUser = User.Create("cache@test.com", "hash");
        _cacheService
            .GetAsync<User>(
                Arg.Is<string>(s => s.Contains(userId.ToString())),
                Arg.Any<CancellationToken>()
            )
            .Returns(cachedUser);

        // Act
        User? result = await _decorator.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cachedUser.Id);
        await _innerRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_ShouldCallInnerRepositoryAndStoreInCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var repoUser = User.Create("repo@test.com", "hash");
        _cacheService
            .GetAsync<User>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _innerRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(repoUser);

        // Act
        User? result = await _decorator.GetByIdAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(repoUser);
        await _innerRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        await _cacheService
            .Received(1)
            .SetAsync(
                Arg.Is<string>(s => s.Contains(userId.ToString())),
                repoUser,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public void Update_ShouldCallInnerUpdateAndInvalidateCache()
    {
        // Arrange
        var user = User.Create("update@test.com", "hash");

        // Act
        _decorator.Update(user);

        // Assert
        _innerRepository.Received(1).Update(user);
        _cacheService.Received(1).RemoveAsync(Arg.Is<string>(s => s.Contains(user.Id.ToString())));
    }
}
