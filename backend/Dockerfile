FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
# Ports are exposed from docker compose
# EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HeatingDataMonitor.API/HeatingDataMonitor.API.csproj", "HeatingDataMonitor.API/"]
COPY ["HeatingDataMonitor.Data/HeatingDataMonitor.Data.csproj", "HeatingDataMonitor.Data/"]
RUN dotnet restore "HeatingDataMonitor.API/HeatingDataMonitor.API.csproj"
COPY . .
WORKDIR "/src/HeatingDataMonitor.API"
RUN dotnet build "HeatingDataMonitor.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HeatingDataMonitor.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HeatingDataMonitor.API.dll"]
