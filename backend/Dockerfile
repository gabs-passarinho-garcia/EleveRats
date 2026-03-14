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

# 1. A JOGADA MESTRA: Traz o juiz pro jogo antes de tudo!
# Isso garante que as regras do seu Zorin OS valham aqui dentro.
COPY [".editorconfig", "."]

# Copia o csproj e restaura as dependências
COPY ["backend/EleveRats.csproj", "backend/"]
RUN dotnet restore "backend/EleveRats.csproj"

# Copia o restante do código e compila a aplicação
COPY ["backend/", "backend/"]
WORKDIR "/src/backend"

# 2. O CINTURÃO DE SEGURANÇA: Ignora os alertas de formatação na hora do publish
RUN dotnet publish "EleveRats.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:TreatWarningsAsErrors=false

# Usa a imagem oficial do Runtime do ASP.NET Core 10 para rodar a API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instala o curl para o healthcheck (e outras ferramentas úteis)
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Adiciona um usuário não-root por segurança (opcional, mas recomendado)
USER $APP_UID

# 3. A REGRA DA LIGA: Força a API a escutar na porta 3000 (o padrão do .NET 10 é 8080)
ENV ASPNETCORE_HTTP_PORTS=3000
EXPOSE 3000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EleveRats.dll"]
