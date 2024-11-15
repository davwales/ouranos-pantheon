FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . ./
RUN dotnet restore ./Talos.Olympus.Gateway.API/Talos.Olympus.Gateway.API.csproj
RUN dotnet publish ./Talos.Olympus.Gateway.API/Talos.Olympus.Gateway.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

COPY --from=build /app/publish .

EXPOSE 8300

ENTRYPOINT ["dotnet", "Talos.Olympus.Gateway.API.dll"]
