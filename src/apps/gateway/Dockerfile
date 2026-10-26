# syntax=docker/dockerfile:1.7-labs
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Project files first so the restore layer is reused until a dependency changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY --parents src/**/*.csproj ./
RUN dotnet restore ./src/apps/gateway/Ouranos.Pantheon.Apps.Gateway/Ouranos.Pantheon.Apps.Gateway.csproj

COPY src ./src
RUN dotnet publish ./src/apps/gateway/Ouranos.Pantheon.Apps.Gateway/Ouranos.Pantheon.Apps.Gateway.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8300
EXPOSE 8300

USER $APP_UID

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -fsS -o /dev/null http://127.0.0.1:8300/health || exit 1

ENTRYPOINT ["dotnet", "Ouranos.Pantheon.Apps.Gateway.dll"]
