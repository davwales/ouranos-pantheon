FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/data-loaders/plutus/Ouranos.Pantheon.DataLoader.Plutus.Consumer/Ouranos.Pantheon.DataLoader.Plutus.Consumer.csproj", "src/data-loaders/plutus/Ouranos.Pantheon.DataLoader.Plutus.Consumer/"]
RUN dotnet restore "src/data-loaders/plutus/Ouranos.Pantheon.DataLoader.Plutus.Consumer/Ouranos.Pantheon.DataLoader.Plutus.Consumer.csproj"
COPY . .
WORKDIR "/src/src/data-loaders/plutus/Ouranos.Pantheon.DataLoader.Plutus.Consumer"
RUN dotnet build "Ouranos.Pantheon.DataLoader.Plutus.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ouranos.Pantheon.DataLoader.Plutus.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ouranos.Pantheon.DataLoader.Plutus.Consumer.dll"]
