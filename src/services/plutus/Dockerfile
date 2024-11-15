FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . ./
RUN dotnet restore ./Talos.Olympus.Service.Plutus.API/Talos.Olympus.Service.Plutus.API.csproj
RUN dotnet publish ./Talos.Olympus.Service.Plutus.API/Talos.Olympus.Service.Plutus.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

COPY --from=build /app/publish .

EXPOSE 8302

ENTRYPOINT ["dotnet", "Talos.Olympus.Service.Plutus.API.dll"]
