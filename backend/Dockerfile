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
# Adiciona um usuário não-root por segurança (opcional, mas recomendado)
USER $APP_UID

COPY --from=build /app/publish .

EXPOSE 3000

ENTRYPOINT ["dotnet", "EleveRats.dll"]
