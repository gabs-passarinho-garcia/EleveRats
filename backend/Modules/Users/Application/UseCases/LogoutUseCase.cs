// <copyright file="LogoutUseCase.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Core.Application.Interfaces;
using EleveRats.Modules.Users.Application.Interfaces;

namespace EleveRats.Modules.Users.Application.UseCases;

/// <summary>
/// Implements the logout flow by delegating the token revocation to the TokenService.
/// </summary>
internal sealed class LogoutUseCase(ITokenService tokenService, IUserContext userContext) : ILogoutUseCase
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserContext _userContext = userContext;

    public async Task ExecuteAsync(string refreshToken)
    {
        if (_userContext.Current is null)
        {
            return;
        }

        await _tokenService.RevokeTokensAsync(
            _userContext.Current.UserId,
            _userContext.Current.Jti,
            refreshToken
        );
    }
}
