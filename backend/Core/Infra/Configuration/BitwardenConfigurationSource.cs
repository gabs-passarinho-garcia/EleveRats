// <copyright file="BitwardenConfigurationSource.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using Microsoft.Extensions.Configuration;

namespace EleveRats.Core.Infra.Configuration;

/// <summary>
/// An <see cref="IConfigurationSource"/> that registers the <see cref="BitwardenConfigurationProvider"/>
/// within the .NET configuration builder pipeline.
/// </summary>
/// <remarks>
/// This source acts as the factory entry point for the Bitwarden integration.
/// Instantiate it via the <c>IConfigurationBuilder.AddBitwardenSecrets()</c>
/// extension method defined in <see cref="BitwardenConfigurationExtensions"/>.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="BitwardenConfigurationSource"/> class.
/// </remarks>
/// <param name="options">The resolved Bitwarden authentication and filtering options.</param>
internal sealed class BitwardenConfigurationSource(BitwardenConfigurationOptions options)
    : IConfigurationSource
{
    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new BitwardenConfigurationProvider(options);
}
