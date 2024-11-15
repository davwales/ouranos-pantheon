FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ./src/services/aphrodite/ ./src/services/aphrodite/
COPY ./src/core/ ./src/core/

RUN dotnet restore ./src/services/aphrodite/Talos.Olympus.Service.Aphrodite.API/Talos.Olympus.Service.Aphrodite.API.csproj
RUN dotnet publish ./src/services/aphrodite/Talos.Olympus.Service.Aphrodite.API/Talos.Olympus.Service.Aphrodite.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8300

ENTRYPOINT ["dotnet", "Talos.Olympus.Service.Aphrodite.API.dll"]
