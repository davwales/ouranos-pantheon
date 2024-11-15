FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ./src/gateway/ ./src/gateway/
COPY ./src/core/ ./src/core/

RUN dotnet restore ./src/gateway/Talos.Olympus.Gateway.API/Talos.Olympus.Gateway.API.csproj
RUN dotnet publish ./src/gateway/Talos.Olympus.Gateway.API/Talos.Olympus.Gateway.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8300

ENTRYPOINT ["dotnet", "Talos.Olympus.Gateway.API.dll"]
