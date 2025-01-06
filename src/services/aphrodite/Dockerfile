FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish ./src/services/aphrodite/Ouranos.Pantheon.Service.Aphrodite.API/Ouranos.Pantheon.Service.Aphrodite.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8300

ENTRYPOINT ["dotnet", "Ouranos.Pantheon.Service.Aphrodite.API.dll"]
