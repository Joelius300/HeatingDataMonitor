FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HeatingDataMonitor.API/HeatingDataMonitor.API.csproj", "HeatingDataMonitor.API/"]
COPY ["HeatingDataMonitor.Database.Read/HeatingDataMonitor.Database.Read.csproj", "HeatingDataMonitor.Database.Read/"]
COPY ["HeatingDataMonitor.Database.Views/HeatingDataMonitor.Database.Views.csproj", "HeatingDataMonitor.Database.Views/"]
COPY ["HeatingDataMonitor.Database.Models/HeatingDataMonitor.Database.Models.csproj", "HeatingDataMonitor.Database.Models/"]
COPY ["HeatingDataMonitor.Database/HeatingDataMonitor.Database.csproj", "HeatingDataMonitor.Database/"]
COPY ["HeatingDataMonitor.Receiver.Shared/HeatingDataMonitor.Receiver.Shared.csproj", "HeatingDataMonitor.Receiver.Shared/"]
# Copying the project files and restoring in it's own step allows docker to cache the dependencies which only rarely change.
RUN dotnet restore "HeatingDataMonitor.API/HeatingDataMonitor.API.csproj"
COPY . .
WORKDIR "/src/HeatingDataMonitor.API"
RUN dotnet build "HeatingDataMonitor.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HeatingDataMonitor.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
# fix logging: dotnet/dotnet-docker#3274
ENV Logging__Console__FormatterName=
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HeatingDataMonitor.API.dll"]
