// <copyright file="CachedProfileRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Domain.Enums;
using EleveRats.Modules.Users.Infra.Persistence.Repositories;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Infra.Persistence.Repositories;

public sealed class CachedProfileRepositoryTests
{
    private readonly IProfileRepository _innerRepository = Substitute.For<IProfileRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IOptions<UsersCacheOptions> _options = Substitute.For<
        IOptions<UsersCacheOptions>
    >();
    private readonly CachedProfileRepository _decorator;

    public CachedProfileRepositoryTests()
    {
        _options.Value.Returns(new UsersCacheOptions { AbsoluteExpirationMinutes = 10 });
        _decorator = new CachedProfileRepository(_innerRepository, _cacheService, _options);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_ShouldCallInnerRepositoryAndStoreAcrossTwoKeys()
    {
        // Arrange
        var profile = Profile.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Warrior",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            ProfileType.Member,
            "warrior_rat"
        );

        _cacheService
            .GetAsync<Profile>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Profile?)null);
        _innerRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        // Act
        Profile? result = await _decorator.GetByIdAsync(profile.Id);

        // Assert
        result.Should().BeEquivalentTo(profile);
        // Verify it was stored using ID key AND UserOrg key
        await _cacheService
            .Received(2)
            .SetAsync(
                Arg.Any<string>(),
                Arg.Is<Profile>(p => p.Id == profile.Id),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task GetByUserIdAndOrganizationIdAsync_WhenCacheHit_ShouldReturnCachedProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var profile = Profile.Create(
            userId,
            orgId,
            "Paladin",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            ProfileType.Member,
            "paladin_rat"
        );
        _cacheService
            .GetAsync<Profile>(
                Arg.Is<string>(s => s.Contains(userId.ToString())),
                Arg.Any<CancellationToken>()
            )
            .Returns(profile);

        // Act
        Profile? result = await _decorator.GetByUserIdAndOrganizationIdAsync(userId, orgId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profile.Id);
        await _innerRepository
            .DidNotReceive()
            .GetByUserIdAndOrganizationIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Update_ShouldCallInnerUpdateAndInvalidateBothCacheKeys()
    {
        // Arrange
        var profile = Profile.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bardo",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            ProfileType.Member,
            "bardo_rat"
        );

        // Act
        _decorator.Update(profile);

        // Assert
        _innerRepository.Received(1).Update(profile);
        // Should remove ID key and UserOrg key
        await _cacheService.Received(2).RemoveAsync(Arg.Any<string>());
    }
}
