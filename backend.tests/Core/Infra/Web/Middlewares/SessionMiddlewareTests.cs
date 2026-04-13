// <copyright file="SessionMiddlewareTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Core.Application.Contexts;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Core.Infra.Web.Middlewares;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Core.Infra.Web.Middlewares;

public class SessionMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ICacheService _cacheService;
    private readonly IUserContext _userContext;
    private readonly SessionMiddleware _sut;

    public SessionMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _cacheService = Substitute.For<ICacheService>();
        _userContext = Substitute.For<IUserContext>();
        _sut = new SessionMiddleware(_next);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotAuthenticated_ShouldProceedToNextWithoutSettingContext()
    {
        // Arrange
        var context = new DefaultHttpContext(); // By default, User.Identity.IsAuthenticated is false

        // Act
        await _sut.InvokeAsync(context, _cacheService, _userContext);

        // Assert
        _userContext.DidNotReceive().Set(Arg.Any<UserSession>());
        await _next.Received(1).Invoke(context);
        _userContext.Received(1).Clear();
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticatedButMissingJti_ShouldReturn401AndNotCallNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new List<Claim>(), "TestAuth"); // Missing JTI
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _sut.InvokeAsync(context, _cacheService, _userContext);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _userContext.DidNotReceive().Set(Arg.Any<UserSession>());
        await _next.DidNotReceive().Invoke(context);
        _userContext.DidNotReceive().Clear();
    }

    [Fact]
    public async Task InvokeAsync_WhenSessionIsRevokedInCache_ShouldReturn401AndNotCallNext()
    {
        // Arrange
        string jti = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Jti, jti)],
            "TestAuth"
        );
        context.User = new ClaimsPrincipal(identity);

        // Simulate revoked or missing session from cache
        _cacheService.GetAsync<string>($"access_id:{jti}").Returns((string?)null);

        // Act
        await _sut.InvokeAsync(context, _cacheService, _userContext);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _userContext.DidNotReceive().Set(Arg.Any<UserSession>());
        await _next.DidNotReceive().Invoke(context);
        _userContext.DidNotReceive().Clear();
    }

    [Fact]
    public async Task InvokeAsync_WhenSessionIsValidWithoutImpersonator_ShouldSetContextAndCallNext()
    {
        // Arrange
        string jti = Guid.NewGuid().ToString();
        var userId = Guid.CreateVersion7();
        var profileId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("profileId", profileId.ToString()),
                new Claim("orgId", orgId.ToString()),
            ],
            "TestAuth"
        );
        context.User = new ClaimsPrincipal(identity);

        // Simulate active session in cache
        _cacheService.GetAsync<string>($"access_id:{jti}").Returns("active");

        // Act
        await _sut.InvokeAsync(context, _cacheService, _userContext);

        // Assert
        _userContext
            .Received(1)
            .Set(
                Arg.Is<UserSession>(u =>
                    u.UserId == userId
                    && u.ProfileId == profileId
                    && u.OrgId == orgId
                    && u.ImpersonatorId == null
                )
            );
        await _next.Received(1).Invoke(context);
        _userContext.Received(1).Clear();
    }

    [Fact]
    public async Task InvokeAsync_WhenSessionIsValidWithImpersonator_ShouldSetContextAndCallNext()
    {
        // Arrange
        string jti = Guid.NewGuid().ToString();
        var userId = Guid.CreateVersion7();
        var profileId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();
        var impersonatorId = Guid.CreateVersion7();

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("profileId", profileId.ToString()),
                new Claim("orgId", orgId.ToString()),
                new Claim("act", impersonatorId.ToString()),
            ],
            "TestAuth"
        );
        context.User = new ClaimsPrincipal(identity);

        _cacheService.GetAsync<string>($"access_id:{jti}").Returns("active");

        // Act
        await _sut.InvokeAsync(context, _cacheService, _userContext);

        // Assert
        _userContext
            .Received(1)
            .Set(
                Arg.Is<UserSession>(u =>
                    u.UserId == userId
                    && u.ProfileId == profileId
                    && u.OrgId == orgId
                    && u.ImpersonatorId == impersonatorId
                    && u.IsImpersonating == true
                )
            );
        await _next.Received(1).Invoke(context);
        _userContext.Received(1).Clear();
    }

    [Fact]
    public async Task InvokeAsync_WhenAnExceptionOccurs_ShouldStillClearContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _next.When(x => x.Invoke(context)).Throw(new Exception("Kaboom"));

        // Act
        Func<Task> act = async () => await _sut.InvokeAsync(context, _cacheService, _userContext);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Kaboom");
        _userContext.Received(1).Clear();
    }
}
