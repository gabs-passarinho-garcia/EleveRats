// <copyright file="IOrganizationRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
/// Defines the contract for Organization persistence operations.
/// </summary>
internal interface IOrganizationRepository
{
    /// <summary>
    /// Retrieves an Organization by its unique identifier.
    /// </summary>
    /// <param name="id">The UUID v7 of the organization.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The domain Organization entity if found; otherwise, null.</returns>
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new Organization entity to the persistence tracking.
    /// </summary>
    /// <param name="organization">The domain Organization entity to insert.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Organization entity in the persistence tracking.
    /// </summary>
    /// <param name="organization">The domain Organization entity to update.</param>
    void Update(Organization organization);
}
