// <copyright file="Constants.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Core;

/// <summary>
/// Centralized repository for application-wide constants.
/// Soli Deo Gloria.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The central gRPC OTLP endpoint for Grafana Alloy (Logs, Metrics, Traces).
    /// </summary>
    public const string AlloyOtlpEndpoint = "http://alloy:4317";

    /// <summary>
    /// The default Redis connection string.
    /// Used as fallback if not provided in configuration.
    /// </summary>
    public const string DefaultRedisConnectionString = "redis:6379";
}
