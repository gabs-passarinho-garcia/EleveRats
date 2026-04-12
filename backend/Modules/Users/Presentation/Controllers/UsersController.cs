// <copyright file="UsersController.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EleveRats.Modules.Users.Presentation.Controllers;

/// <summary>
/// Controller responsável pela gestão e operações táticas de usuários.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize] // O Sentinela (Middleware) já garante o contexto aqui
internal class UsersController(IImpersonateUserUseCase impersonateUseCase) : ControllerBase
{
    private const string _unknownIpAddress = "unknown";
    private readonly IImpersonateUserUseCase _impersonateUseCase = impersonateUseCase;

    /// <summary>
    /// Ativa o Modo Sombra: Permite que um Master User assuma o controle de outra conta.
    /// </summary>
    /// <param name="targetUserId">ID do guerreiro que terá a conta assumida.</param>
    /// <returns>Um novo par de tokens (JWT com claim 'act' e Refresh Token).</returns>
    [HttpPost("impersonate/{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Impersonate(Guid targetUserId)
    {
        // Extração de metadados de infraestrutura (Responsabilidade da Presentation)
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? _unknownIpAddress;

        // Delegação total da orquestração para o Use Case
        TokenResponse result = await _impersonateUseCase.ExecuteAsync(targetUserId, ipAddress);

        return Ok(result);
    }
}
