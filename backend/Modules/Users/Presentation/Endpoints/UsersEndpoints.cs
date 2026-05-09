// <copyright file="UsersEndpoints.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.UseCases;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EleveRats.Modules.Users.Presentation.Endpoints;

/// <summary>
/// Mapeamento de endpoints para o módulo de usuários usando Minimal APIs.
/// </summary>
public static class UsersEndpoints
{
    private const string _unknownIpAddress = "unknown";

    /// <summary>
    /// Configura as rotas do módulo de usuários.
    /// </summary>
    /// <param name="app">O builder de rotas.</param>
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("api/users").RequireAuthorization(); // O Sentinela (Middleware) já garante o contexto aqui

        // Impersonate User
        // Ativa o Modo Sombra: Permite que um Master User assuma o controle de outra conta.
        group
            .MapPost(
                "impersonate/{targetProfileId:guid}",
                async (
                    Guid targetProfileId,
                    IImpersonateUserUseCase impersonateUseCase,
                    HttpContext context
                ) =>
                {
                    // Extração de metadados de infraestrutura (Responsabilidade da Presentation)
                    string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? _unknownIpAddress;

                    // Delegação total da orquestração para o Use Case
                    TokenResponse result = await impersonateUseCase.ExecuteAsync(
                        targetProfileId,
                        ipAddress
                    );

                    return Results.Ok(result);
                }
            )
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithSummary("Impersonate User")
            .WithDescription(
                "Ativa o Modo Sombra: Permite que um Master User assuma o controle de outra conta."
            );
    }
}
