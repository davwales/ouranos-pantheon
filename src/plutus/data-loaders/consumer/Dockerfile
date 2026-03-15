FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish ./src/plutus/data-loaders/consumer/Ouranos.Pantheon.Plutus.DataLoader.Consumer/Ouranos.Pantheon.Plutus.DataLoader.Consumer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Ouranos.Pantheon.Plutus.DataLoader.Consumer.dll"]