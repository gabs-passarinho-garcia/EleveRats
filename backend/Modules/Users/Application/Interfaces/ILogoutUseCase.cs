// <copyright file="ILogoutUseCase.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.Threading.Tasks;

namespace EleveRats.Modules.Users.Application.Interfaces;

/// <summary>
/// Defines the use case for securely ending a user session.
/// </summary>
public interface ILogoutUseCase
{
    /// <summary>
    /// Executes the logout process, revoking both access and refresh tokens.
    /// </summary>
    /// <param name="refreshToken">The raw refresh token provided by the client.</param>
    /// <returns>A task representing the asynchronous logout operation.</returns>
    Task ExecuteAsync(string refreshToken);
}
