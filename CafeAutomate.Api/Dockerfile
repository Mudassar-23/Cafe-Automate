# ── Backend — multi-stage .NET 8 build ──────────────────────────

# Stage 1: restore & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cached unless .csproj changes)
COPY CafeAutomate.Api.csproj .
RUN dotnet restore --locked-mode 2>/dev/null || dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a writable directory for the SQLite database
RUN mkdir -p /app/Data && chmod 777 /app/Data

# Copy published output
COPY --from=build /app/publish .

# Run as a non-root user for better security
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# ASP.NET Core listens on 8080 inside the container
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CafeAutomate.Api.dll"]
