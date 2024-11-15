FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . ./
RUN dotnet restore ./Talos.Olympus.Service.Aphrodite.API/Talos.Olympus.Service.Aphrodite.API.csproj
RUN dotnet publish ./Talos.Olympus.Service.Aphrodite.API/Talos.Olympus.Service.Aphrodite.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

COPY --from=build /app/publish .

EXPOSE 8301

ENTRYPOINT ["dotnet", "Talos.Olympus.Service.Aphrodite.API.dll"]
