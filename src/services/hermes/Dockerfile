FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish ./src/services/hermes/Ouranos.Pantheon.Service.Hermes.API/Ouranos.Pantheon.Service.Hermes.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8300

ENTRYPOINT ["dotnet", "Ouranos.Pantheon.Service.Hermes.API.dll"]
