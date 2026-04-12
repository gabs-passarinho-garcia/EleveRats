// <copyright file="ImpersonateUserUseCase.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Core.Application.Contexts;
using EleveRats.Core.Application.Interfaces;
using EleveRats.Modules.Users.Application.Interfaces;
using EleveRats.Modules.Users.Application.Repositories;
using EleveRats.Modules.Users.Domain.Entities;

namespace EleveRats.Modules.Users.Application.UseCases;

/// <summary>
/// Orquestra a emissão de um token de impersonificação, garantindo que as regras
/// de negócio (usuário alvo ativo, perfil existente) sejam respeitadas.
/// </summary>
internal sealed class ImpersonateUserUseCase(
    ITokenService tokenService,
    IUserContext userContext,
    IUserRepository userRepository,
    IProfileRepository profileRepository
) : IImpersonateUserUseCase
{
    public async Task<TokenResponse> ExecuteAsync(Guid targetProfileId, string ipAddress)
    {
        // 1. O Sentinela: Valida quem está assumindo o controle
        UserSession? currentUser =
            userContext.Current
            ?? throw new UnauthorizedAccessException(
                "Acesso negado: Contexto de usuário não encontrado."
            );

        User masterUser =
            await userRepository.GetByIdAsync(currentUser.UserId)
            ?? throw new UnauthorizedAccessException("Acesso negado: Mestre não encontrado.");

        if (!masterUser.IsMaster)
        {
            throw new UnauthorizedAccessException("Apenas Masters podem usar a Sombra.");
        }

        // 3. Busca o Perfil do Alvo para obter o ProfileId e OrgId reais
        Profile targetProfile =
            await profileRepository.GetByIdAsync(targetProfileId)
            ?? throw new InvalidOperationException(
                "O Alvo não possui um perfil de combate configurado."
            );

        if (!targetProfile.IsMember)
        {
            throw new InvalidOperationException("O perfil alvo não está ativo.");
        }

        User targetUser =
            await userRepository.GetByIdAsync(targetProfile.UserId)
            ?? throw new InvalidOperationException("O usuário alvo não foi encontrado.");

        if (!targetUser.IsActive)
        {
            throw new InvalidOperationException("O usuário alvo não está ativo.");
        }

        // 4. A Sombra Age: Gera o Token com os dados do Alvo, mas assinando o "act" com o ID do Master
        return await tokenService.GenerateTokenPairAsync(
            userId: targetProfile.UserId,
            profileId: targetProfile.Id,
            orgId: targetProfile.OrganizationId,
            impersonatorId: currentUser.UserId,
            ipAddress: ipAddress
        );
    }
}
