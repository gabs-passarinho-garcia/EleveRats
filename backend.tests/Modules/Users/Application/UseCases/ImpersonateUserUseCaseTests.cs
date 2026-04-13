// <copyright file="ImpersonateUserUseCaseTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Core.Application.Contexts;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Application.UseCases;
using EleveRats.Modules.Users.Domain.Entities;
using EleveRats.Modules.Users.Domain.Enums;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Application.UseCases;

public class ImpersonateUserUseCaseTests
{
    private readonly ITokenService _tokenService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly ImpersonateUserUseCase _sut;

    public ImpersonateUserUseCaseTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        _userContext = Substitute.For<IUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _profileRepository = Substitute.For<IProfileRepository>();

        _sut = new ImpersonateUserUseCase(
            _tokenService,
            _userContext,
            _userRepository,
            _profileRepository
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoUserContext_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _userContext.Current.Returns((UserSession?)null);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(Guid.CreateVersion7(), "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Acesso negado: Contexto de usuário não encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenMasterUserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        _userRepository.GetByIdAsync(masterSession.UserId).Returns((User?)null);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(Guid.CreateVersion7(), "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Acesso negado: Mestre não encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsNotMaster_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        var notMasterUser = User.Create("notmaster@test.com", "hash", isMaster: false);
        _userRepository.GetByIdAsync(masterSession.UserId).Returns(notMasterUser);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(Guid.CreateVersion7(), "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Apenas Masters podem usar a Sombra.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetProfileNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        var masterUser = User.Create("master@test.com", "hash", isMaster: true);
        _userRepository.GetByIdAsync(masterSession.UserId).Returns(masterUser);

        var targetProfileId = Guid.CreateVersion7();
        _profileRepository.GetByIdAsync(targetProfileId).Returns((Profile?)null);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(targetProfileId, "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("O Alvo não possui um perfil de combate configurado.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetProfileIsNotMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        var masterUser = User.Create("master@test.com", "hash", isMaster: true);
        _userRepository.GetByIdAsync(masterSession.UserId).Returns(masterUser);

        var targetProfileId = Guid.CreateVersion7();

        // Create an inactive profile by reconstituting
        var inactiveProfile = Profile.Reconstitute(
            targetProfileId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Target User",
            new DateOnly(2000, 1, 1),
            Gender.Male,
            isMember: false,
            ProfileType.Member,
            DateTimeOffset.UtcNow,
            "creator",
            null,
            null
        );

        _profileRepository.GetByIdAsync(targetProfileId).Returns(inactiveProfile);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(targetProfileId, "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("O perfil alvo não está ativo.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetUserNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        var masterUser = User.Create("master@test.com", "hash", isMaster: true);
        _userRepository.GetByIdAsync(masterSession.UserId).Returns(masterUser);

        var targetProfileId = Guid.CreateVersion7();
        var targetProfile = Profile.Reconstitute(
            targetProfileId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Target User",
            new DateOnly(2000, 1, 1),
            Gender.Male,
            isMember: true,
            ProfileType.Member,
            DateTimeOffset.UtcNow,
            "creator",
            null,
            null
        );

        _profileRepository.GetByIdAsync(targetProfileId).Returns(targetProfile);
        _userRepository.GetByIdAsync(targetProfile.UserId).Returns((User?)null);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(targetProfileId, "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("O usuário alvo não foi encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetUserIsNotActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        var masterUser = User.Create("master@test.com", "hash", isMaster: true);
        _userRepository.GetByIdAsync(masterSession.UserId).Returns(masterUser);

        var targetProfileId = Guid.CreateVersion7();
        var targetProfile = Profile.Reconstitute(
            targetProfileId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Target User",
            new DateOnly(2000, 1, 1),
            Gender.Male,
            isMember: true,
            ProfileType.Member,
            DateTimeOffset.UtcNow,
            "creator",
            null,
            null
        );

        _profileRepository.GetByIdAsync(targetProfileId).Returns(targetProfile);

        var targetUser = User.Reconstitute(
            targetProfile.UserId,
            "target@test.com",
            "hash",
            isActive: false,
            isMaster: false,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null
        );
        _userRepository.GetByIdAsync(targetProfile.UserId).Returns(targetUser);

        // Act
        Func<Task<TokenResponse>> act = async () =>
            await _sut.ExecuteAsync(targetProfileId, "127.0.0.1");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("O usuário alvo não está ativo.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllValidationsPass_ShouldGenerateAndReturnTokenAsync()
    {
        // Arrange
        var masterSession = new UserSession(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "trace",
            "ip",
            null
        );
        _userContext.Current.Returns(masterSession);

        var masterUser = User.Create("master@test.com", "hash", isMaster: true);
        _userRepository.GetByIdAsync(masterSession.UserId).Returns(masterUser);

        var targetProfileId = Guid.CreateVersion7();
        var targetProfile = Profile.Reconstitute(
            targetProfileId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(), // target userId
            "Target User",
            new DateOnly(2000, 1, 1),
            Gender.Male,
            isMember: true,
            ProfileType.Member,
            DateTimeOffset.UtcNow,
            "creator",
            null,
            null
        );

        _profileRepository.GetByIdAsync(targetProfileId).Returns(targetProfile);

        var targetUser = User.Reconstitute(
            targetProfile.UserId,
            "target@test.com",
            "hash",
            isActive: true,
            isMaster: false,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null
        );
        _userRepository.GetByIdAsync(targetProfile.UserId).Returns(targetUser);

        TokenResponse expectedTokenResponse = default!;

        string ipAddress = "192.168.0.1";

        _tokenService
            .GenerateTokenPairAsync(
                targetProfile.UserId,
                targetProfile.Id,
                targetProfile.OrganizationId,
                masterSession.UserId, // The impersonator ID
                ipAddress
            )
            .Returns(Task.FromResult(expectedTokenResponse));

        // Act
        TokenResponse result = await _sut.ExecuteAsync(targetProfileId, ipAddress);

        // Assert
        result.Should().Be(expectedTokenResponse);

        await _tokenService
            .Received(1)
            .GenerateTokenPairAsync(
                targetProfile.UserId,
                targetProfile.Id,
                targetProfile.OrganizationId,
                masterSession.UserId,
                ipAddress
            );
    }
}
