// <copyright file="CachedOrganizationRepositoryTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

public sealed class CachedOrganizationRepositoryTests
{
    private readonly IOrganizationRepository _innerRepository =
        Substitute.For<IOrganizationRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IOptions<UsersCacheOptions> _options = Substitute.For<
        IOptions<UsersCacheOptions>
    >();
    private readonly CachedOrganizationRepository _decorator;

    public CachedOrganizationRepositoryTests()
    {
        _options.Value.Returns(new UsersCacheOptions { AbsoluteExpirationMinutes = 15 });
        _decorator = new CachedOrganizationRepository(_innerRepository, _cacheService, _options);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_ShouldCallInnerAndPopulateCache()
    {
        // Arrange
        var organization = Organization.Create("EleveRats", "eleverats");
        _cacheService
            .GetAsync<Organization>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Organization?)null);
        _innerRepository
            .GetByIdAsync(organization.Id, Arg.Any<CancellationToken>())
            .Returns(organization);

        // Act
        Organization? result = await _decorator.GetByIdAsync(organization.Id);

        // Assert
        result.Should().BeEquivalentTo(organization);
        await _cacheService
            .Received(1)
            .SetAsync(
                Arg.Is<string>(s => s.Contains(organization.Id.ToString())),
                organization,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public void Update_ShouldCallInnerAndRemoveFromCache()
    {
        // Arrange
        var organization = Organization.Create("Update-Org", "update");

        // Act
        _decorator.Update(organization);

        // Assert
        _innerRepository.Received(1).Update(organization);
        _cacheService
            .Received(1)
            .RemoveAsync(Arg.Is<string>(s => s.Contains(organization.Id.ToString())));
    }
}
