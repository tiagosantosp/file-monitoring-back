# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["FileMonitoring.API/FileMonitoring.API.csproj", "FileMonitoring/"]
COPY ["FileMonitoring.Application/FileMonitoring.Application.csproj", "FileMonitoring.Application/"]
COPY ["FileMonitoring.Domain/FileMonitoring.Domain.csproj", "FileMonitoring.Domain/"]
COPY ["FileMonitoring.Infrastructure/FileMonitoring.Infrastructure.csproj", "FileMonitoring.Infrastructure/"]

RUN dotnet restore "FileMonitoring.API/FileMonitoring.API.csproj"

# Copiar todo o código e compilar
COPY . .
WORKDIR "/src/FileMonitoring.API"
RUN dotnet build "FileMonitoring.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "FileMonitoring.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

# Criar pasta de Backups
RUN mkdir -p /app/Backups

ENTRYPOINT ["dotnet", "FileMonitoring.API.dll"]