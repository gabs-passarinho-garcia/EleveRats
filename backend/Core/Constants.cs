// <copyright file="Constants.cs" company="PlaceholderCompany">
// Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// </copyright>

namespace EleveRats.Core;

/// <summary>
/// Centralized repository for application-wide constants.
/// Soli Deo Gloria.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The gRPC OTLP endpoint for Grafana Tempo tracing.
    /// </summary>
    public const string TempoEndpoint = "http://tempo:4317";
}
