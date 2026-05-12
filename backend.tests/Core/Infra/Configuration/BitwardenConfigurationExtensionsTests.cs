// <copyright file="BitwardenConfigurationExtensionsTests.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using AwesomeAssertions;
using EleveRats.Core.Infra.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EleveRats.Tests.Core.Infra.Configuration;

/// <summary>
/// Unit tests for the BitwardenConfigurationExtensions class.
/// </summary>
public sealed class BitwardenConfigurationExtensionsTests : IDisposable
{
    private const string _accessToken = "BITWARDEN_ACCESS_TOKEN";
    private const string _orgId = "BITWARDEN_ORGANIZATION_ID";
    private const string _projId = "BITWARDEN_PROJECT_ID";

    public BitwardenConfigurationExtensionsTests()
    {
        // Clear environment variables before each test
        Environment.SetEnvironmentVariable(_accessToken, null);
        Environment.SetEnvironmentVariable(_orgId, null);
        Environment.SetEnvironmentVariable(_projId, null);
    }

    public void Dispose()
    {
        // Cleanup after each test
        Environment.SetEnvironmentVariable(_accessToken, null);
        Environment.SetEnvironmentVariable(_orgId, null);
        Environment.SetEnvironmentVariable(_projId, null);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void AddBitwardenSecrets_WhenAccessTokenIsMissing_ShouldNotAddSource()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        Environment.SetEnvironmentVariable(_orgId, Guid.NewGuid().ToString());
        Environment.SetEnvironmentVariable(_projId, Guid.NewGuid().ToString());

        // Act
        builder.AddBitwardenSecrets();

        // Assert
        builder.Sources.Should().BeEmpty("because BITWARDEN_ACCESS_TOKEN was not set");
    }

    [Fact]
    public void AddBitwardenSecrets_WhenOrganizationIdIsMissing_ShouldNotAddSource()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        Environment.SetEnvironmentVariable(_accessToken, "some-token");
        Environment.SetEnvironmentVariable(_projId, Guid.NewGuid().ToString());

        // Act
        builder.AddBitwardenSecrets();

        // Assert
        builder.Sources.Should().BeEmpty("because BITWARDEN_ORGANIZATION_ID was not set");
    }

    [Fact]
    public void AddBitwardenSecrets_WhenProjectIdIsMissing_ShouldNotAddSource()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        Environment.SetEnvironmentVariable(_accessToken, "some-token");
        Environment.SetEnvironmentVariable(_orgId, Guid.NewGuid().ToString());

        // Act
        builder.AddBitwardenSecrets();

        // Assert
        builder.Sources.Should().BeEmpty("because BITWARDEN_PROJECT_ID was not set");
    }

    [Fact]
    public void AddBitwardenSecrets_WhenOrganizationIdIsInvalid_ShouldNotAddSource()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        Environment.SetEnvironmentVariable(_accessToken, "some-token");
        Environment.SetEnvironmentVariable(_orgId, "not-a-guid");
        Environment.SetEnvironmentVariable(_projId, Guid.NewGuid().ToString());

        // Act
        builder.AddBitwardenSecrets();

        // Assert
        builder.Sources.Should().BeEmpty("because BITWARDEN_ORGANIZATION_ID is an invalid GUID");
    }

    [Fact]
    public void AddBitwardenSecrets_WhenAllEnvVarsAreSet_ShouldAddSource()
    {
        // Arrange
        var builder = new ConfigurationBuilder();
        Environment.SetEnvironmentVariable(_accessToken, "some-token");
        Environment.SetEnvironmentVariable(_orgId, Guid.NewGuid().ToString());
        Environment.SetEnvironmentVariable(_projId, Guid.NewGuid().ToString());

        // Act
        builder.AddBitwardenSecrets();

        // Assert
        builder.Sources.Should().ContainSingle(s => s is BitwardenConfigurationSource);
    }
}
