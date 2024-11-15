FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ./src/services/plutus/ ./src/services/plutus/
COPY ./src/core/ ./src/core/

RUN dotnet restore ./src/services/plutus/Talos.Olympus.Service.Plutus.API/Talos.Olympus.Service.Plutus.API.csproj
RUN dotnet publish ./src/services/plutus/Talos.Olympus.Service.Plutus.API/Talos.Olympus.Service.Plutus.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8300

ENTRYPOINT ["dotnet", "Talos.Olympus.Service.Plutus.API.dll"]
