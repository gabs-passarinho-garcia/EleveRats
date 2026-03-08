# Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as
# published by the Free Software Foundation, either version 3 of the
# License, or (at your option) any later version.
# 
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
# 
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.


# Usa a imagem oficial do SDK do .NET 10 como ambiente de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o csproj e restaura as dependências
COPY ["backend/EleveRats.csproj", "backend/"]
RUN dotnet restore "backend/EleveRats.csproj"

# Copia o restante do código e compila a aplicação
COPY ["backend/", "backend/"]
WORKDIR "/src/backend"
RUN dotnet publish "EleveRats.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Usa a imagem oficial do Runtime do ASP.NET Core 10 para rodar a API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instala o curl para o healthcheck (e outras ferramentas úteis)
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Adiciona um usuário não-root por segurança (opcional, mas recomendado)
USER $APP_UID

COPY --from=build /app/publish .

EXPOSE 3000

ENTRYPOINT ["dotnet", "EleveRats.dll"]
