FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish ./src/plutus/data-loaders/ffxiv/Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer/Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.dll"]