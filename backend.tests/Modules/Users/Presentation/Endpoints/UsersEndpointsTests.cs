// <copyright file="UsersEndpointsTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Presentation.Contracts;
using EleveRats.Tests.Core.Infra.Web;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Modules.Users.Presentation.Endpoints;

public class UsersEndpointsTests : IClassFixture<EleveRatsApplicationFactory>
{
    private readonly EleveRatsApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointsTests(EleveRatsApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_Logout_WithValidToken_ReturnsNoContent()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Test",
            "dummy-token"
        );
        var request = new LogoutRequest("valid-refresh-token");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Post_Logout_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        // No authorization header
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new LogoutRequest("valid-refresh-token");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Impersonate_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var targetProfileId = Guid.CreateVersion7();

        // Act
        HttpResponseMessage response = await _client.PostAsync(
            $"/api/users/impersonate/{targetProfileId}",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
