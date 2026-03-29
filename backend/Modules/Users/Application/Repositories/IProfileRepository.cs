// <copyright file="IProfileRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Threading;
using System.Threading.Tasks;
using EleveRats.Modules.Users.Domain.Entities;

namespace EleveRats.Modules.Users.Application.Repositories;

/// <summary>
/// Defines the contract for Profile persistence operations.
/// </summary>
internal interface IProfileRepository
{
    /// <summary>
    /// Retrieves a Profile by its unique identifier.
    /// </summary>
    /// <param name="id">The UUID v7 of the profile.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The domain Profile entity if found; otherwise, null.</returns>
    Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Profile by the combination of User ID and Organization ID.
    /// A user should have only one profile per organization.
    /// </summary>
    /// <param name="userId">The global User ID.</param>
    /// <param name="organizationId">The Organization ID.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The domain Profile entity if found; otherwise, null.</returns>
    Task<Profile?> GetByUserIdAndOrganizationIdAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new Profile entity to the persistence tracking.
    /// </summary>
    /// <param name="profile">The domain Profile entity to insert.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Profile entity in the persistence tracking.
    /// </summary>
    /// <param name="profile">The domain Profile entity to update.</param>
    void Update(Profile profile);
}
