# Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as
# published by the Free Software Foundation, either version 3 of the
# License, or (at your option) any later version.

# 1. BUILD STAGE: Using the Alpine SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Install dependencies required for ReadyToRun compilation on Alpine
RUN apk add --no-cache clang build-base zlib-dev

COPY [".editorconfig", "."]
COPY ["backend/EleveRats.csproj", "backend/"]

# Restore dependencies targeting the specific Alpine architecture
RUN dotnet restore "backend/EleveRats.csproj" -r linux-musl-x64

COPY ["backend/", "backend/"]
WORKDIR "/src/backend"

# Publish with ReadyToRun enabled to reduce JIT compilation at startup.
# We keep self-contained false to reuse the optimized ASP.NET runtime image.
RUN dotnet publish "EleveRats.csproj" \
    -c Release \
    -r linux-musl-x64 \
    --self-contained false \
    -p:PublishReadyToRun=true \
    -p:TreatWarningsAsErrors=false \
    -o /app/publish

# 2. RUNTIME STAGE: Using the lightweight Alpine ASP.NET image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Install curl for Render health checks and timezone data if necessary
USER root
RUN apk add --no-cache curl tzdata && \
    rm -rf /var/cache/apk/*

# Downgrade privileges for security compliance
USER $APP_UID

# Enforce the application to listen on port 3000
ENV ASPNETCORE_HTTP_PORTS=3000
EXPOSE 3000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EleveRats.dll"]
