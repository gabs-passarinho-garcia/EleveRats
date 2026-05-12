// <copyright file="BitwardenSecret.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Core.Infra.Configuration;

/// <summary>
/// A lightweight representation of a Bitwarden secret with its value and metadata.
/// </summary>
/// <param name="Id">The unique identifier of the secret.</param>
/// <param name="Key">The key/name of the secret.</param>
/// <param name="Value">The sensitive value of the secret.</param>
/// <param name="ProjectId">The project ID this secret belongs to (optional in BSM).</param>
internal record BitwardenSecret(Guid Id, string Key, string Value, Guid? ProjectId);
