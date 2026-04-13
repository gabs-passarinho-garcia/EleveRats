// <copyright file="UserSession.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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

namespace EleveRats.Core.Application.Contexts;

// O Contrato de Sessão
internal record UserSession(
    Guid UserId,
    Guid ProfileId,
    Guid OrgId,
    string TraceId,
    string IpAddress,
    Guid? ImpersonatorId
)
{
    // Facilita a verificação tática se é um Master User disfarçado
    public bool IsImpersonating => ImpersonatorId.HasValue;
}
