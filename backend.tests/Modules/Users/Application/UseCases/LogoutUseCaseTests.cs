// <copyright file="LogoutUseCaseTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// </copyright>

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Core.Application.Contexts;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.UseCases;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Application.UseCases;

public class LogoutUseCaseTests
{
    private readonly ITokenService _tokenService;
    private readonly IUserContext _userContext;
    private readonly LogoutUseCase _sut;

    public LogoutUseCaseTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        _userContext = Substitute.For<IUserContext>();

        _sut = new LogoutUseCase(_tokenService, _userContext);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContextIsNull_ShouldReturnWithoutCallingTokenService()
    {
        // Arrange
        _userContext.Current.Returns((UserSession?)null);
        string refreshToken = "some-refresh-token";

        // Act
        await _sut.ExecuteAsync(refreshToken);

        // Assert
        await _tokenService
            .DidNotReceiveWithAnyArgs()
            .RevokeTokensAsync(default, default!, default!);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContextIsValid_ShouldCallRevokeTokensAsyncWithCorrectParameters()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        string jti = Guid.CreateVersion7().ToString();
        var session = new UserSession(
            userId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null,
            jti
        );
        _userContext.Current.Returns(session);

        string refreshToken = "valid-refresh-token";

        // Act
        await _sut.ExecuteAsync(refreshToken);

        // Assert
        await _tokenService.Received(1).RevokeTokensAsync(userId, jti, refreshToken);
    }
}
