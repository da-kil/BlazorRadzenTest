# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY ["ti8m.BeachBreak.sln", "./"]

# Copy all project files for restore
COPY ["01_Domain/ti8m.BeachBreak.Domain/ti8m.BeachBreak.Domain.csproj", "01_Domain/ti8m.BeachBreak.Domain/"]
COPY ["02_Application/ti8m.BeachBreak.Application.Query/ti8m.BeachBreak.Application.Query.csproj", "02_Application/ti8m.BeachBreak.Application.Query/"]
COPY ["03_Infrastructure/ti8m.BeachBreak.QueryApi/ti8m.BeachBreak.QueryApi.csproj", "03_Infrastructure/ti8m.BeachBreak.QueryApi/"]
COPY ["03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/ti8m.BeachBreak.Infrastructure.Marten.csproj", "03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/"]
COPY ["04_Core/ti8m.BeachBreak.Core/ti8m.BeachBreak.Core.csproj", "04_Core/ti8m.BeachBreak.Core/"]
COPY ["04_Core/ti8m.BeachBreak.Core.Infrastructure/ti8m.BeachBreak.Core.Infrastructure.csproj", "04_Core/ti8m.BeachBreak.Core.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "03_Infrastructure/ti8m.BeachBreak.QueryApi/ti8m.BeachBreak.QueryApi.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/03_Infrastructure/ti8m.BeachBreak.QueryApi"
RUN dotnet build "ti8m.BeachBreak.QueryApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ti8m.BeachBreak.QueryApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables (will be overridden by Kubernetes)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=60s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ti8m.BeachBreak.QueryApi.dll"]
