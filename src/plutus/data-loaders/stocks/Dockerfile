FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish ./src/plutus/data-loaders/stocks/Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer/Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.dll"]