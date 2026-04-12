// <copyright file="UserContext.cs" company="Gabriel Passarinho Garcia and EleveRats Team">
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
using EleveRats.Core.Application.Interfaces;

namespace EleveRats.Core.Application.Contexts;

// A Implementação do Motor usando AsyncLocal
internal class UserContext : IUserContext
{
    // AsyncLocal garante que os dados fluam apenas na mesma cadeia de execução (thread assíncrona)
    private static readonly AsyncLocal<UserSession?> _session = new();

    public UserSession? Current => _session.Value;

    public void Set(UserSession session) => _session.Value = session;

    public void Clear() => _session.Value = null;
}
