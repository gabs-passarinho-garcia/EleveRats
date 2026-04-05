// <copyright file="IRefreshTokenRepository.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

using EleveRats.Modules.Users.Domain.Entities;

namespace EleveRats.Modules.Users.Application.Repositories;

/// <summary>
/// Defines the contract for RefreshToken persistence operations.
/// This interface resides in the Application layer, adhering to the Dependency Inversion Principle.
/// The Infrastructure layer is responsible for providing the concrete implementation.
/// </summary>
internal interface IRefreshTokenRepository
{
    /// <summary>
    /// Persists a new refresh token to the store.
    /// </summary>
    /// <param name="token">The <see cref="RefreshToken"/> domain entity to persist.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an active refresh token by its owner and token hash.
    /// </summary>
    /// <param name="userId">The ID of the user who owns the token.</param>
    /// <param name="tokenHash">The SHA-256 hash of the raw token string.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The <see cref="RefreshToken"/> domain entity if found; otherwise, null.</returns>
    Task<RefreshToken?> FindByUserAndHashAsync(
        Guid userId,
        string tokenHash,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Persists the updated state of an existing refresh token (e.g., after revocation).
    /// </summary>
    /// <param name="token">The <see cref="RefreshToken"/> domain entity with updated state.</param>
    void Update(RefreshToken token);
}
