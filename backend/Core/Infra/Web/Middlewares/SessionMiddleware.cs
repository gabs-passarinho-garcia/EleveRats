// <copyright file="SessionMiddleware.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using EleveRats.Core.Application.Contexts;
using EleveRats.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EleveRats.Core.Infra.Web.Middlewares;

internal class SessionMiddleware(RequestDelegate next)
{
    private const string _unknownIpAddress = "unknown";

    public async Task InvokeAsync(
        HttpContext context,
        ICacheService cacheService,
        IUserContext userContext
    )
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // O Sentinela checa o cache. Se não encontrar "active", a sessão foi morta/revogada
            string? sessionStatus = await cacheService.GetAsync<string>($"access_id:{jti}");
            if (sessionStatus == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Extração segura dos dados bélicos do JWT (Defesa em Profundidade)
            string? subClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            string? profileIdClaim = context.User.FindFirst("profileId")?.Value;
            string? orgIdClaim = context.User.FindFirst("orgId")?.Value;

            if (!Guid.TryParse(subClaim, out Guid userId) ||
                !Guid.TryParse(profileIdClaim, out Guid profileId) ||
                !Guid.TryParse(orgIdClaim, out Guid orgId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // O claim "act" carrega a Sombra (ImpersonatorId) - Validamos se existir
            string? actClaim = context.User.FindFirst("act")?.Value;
            Guid? impersonatorId = null;

            if (actClaim != null)
            {
                if (!Guid.TryParse(actClaim, out Guid parsedImpersonatorId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                impersonatorId = parsedImpersonatorId;
            }

            string traceId = context.TraceIdentifier;
            string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? _unknownIpAddress;

            // Popula o contexto
            userContext.Set(
                new UserSession(userId, profileId, orgId, traceId, ipAddress, impersonatorId)
            );
        }

        try
        {
            await next(context);
        }
        finally
        {
            // Regra de Ouro: Destruir as provas após a request para evitar vazamento entre threads no Pool
            userContext.Clear();
        }
    }
}
