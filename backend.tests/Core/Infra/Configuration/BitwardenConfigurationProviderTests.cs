// <copyright file="BitwardenConfigurationProviderTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using EleveRats.Core.Infra.Configuration;
using NSubstitute;
using Xunit;

namespace EleveRats.Tests.Core.Infra.Configuration;

/// <summary>
/// Unit tests for the BitwardenConfigurationProvider class.
/// </summary>
public sealed class BitwardenConfigurationProviderTests
{
    private readonly BitwardenConfigurationOptions _options;
    private readonly IBitwardenClientWrapper _wrapper;
    private readonly BitwardenConfigurationProvider _provider;

    public BitwardenConfigurationProviderTests()
    {
        _options = new BitwardenConfigurationOptions
        {
            AccessToken = "test-token",
            OrganizationId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
        };
        _wrapper = Substitute.For<IBitwardenClientWrapper>();
        _provider = new BitwardenConfigurationProvider(_options, _wrapper);
    }

    [Fact]
    public void Load_WhenProjectHasNoSecrets_ShouldNotLoadAnyData()
    {
        // Arrange
        _wrapper.ListSecrets(_options.ProjectId).Returns([]);

        // Act
        _provider.Load();

        // Assert
        _wrapper.Received(1).Authenticate(_options.AccessToken);
        _wrapper.Received(1).ListSecrets(_options.ProjectId);
        _wrapper.DidNotReceive().GetSecretsByIds(Arg.Any<Guid[]>());
    }

    [Fact]
    public void Load_WhenSecretsExist_ShouldMapKeys()
    {
        // Arrange
        var secretId1 = Guid.NewGuid();
        var secretId2 = Guid.NewGuid();

        BitwardenSecretIdentifier id1 = new(secretId1, "Key1");
        BitwardenSecretIdentifier id2 = new(secretId2, "Key2");

        BitwardenSecret res1 = new(secretId1, "Service__Setting", "Value1", _options.ProjectId);
        BitwardenSecret res2 = new(secretId2, "Other__Setting", "Value2", _options.ProjectId);

        _wrapper.ListSecrets(_options.ProjectId).Returns([id1, id2]);
        _wrapper.GetSecretsByIds(Arg.Any<Guid[]>()).Returns([res1, res2]);

        // Act
        _provider.Load();

        // Assert
        _provider.TryGet("Service:Setting", out string? value1).Should().BeTrue();
        value1.Should().Be("Value1");

        _provider.TryGet("Other:Setting", out string? value2).Should().BeTrue();
        value2.Should().Be("Value2");
    }

    [Fact]
    public void Load_WhenSdkThrows_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _wrapper
            .When(x => x.Authenticate(Arg.Any<string>()))
            .Do(_ => throw new Exception("SDK Error"));

        // Act
        Action act = () => _provider.Load();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*SDK Error*");
    }
}
