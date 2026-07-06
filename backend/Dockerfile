FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TownManager.Api/*.csproj TownManager.Api/
COPY TownManager.Application/*.csproj TownManager.Application/
COPY TownManager.Infrastructure/*.csproj TownManager.Infrastructure/
COPY TownManager.Domain/*.csproj TownManager.Domain/
RUN dotnet restore TownManager.Api/TownManager.Api.csproj

COPY . .

WORKDIR /src/TownManager.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TownManager.Api.dll"]
