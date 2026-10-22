# syntax=docker/dockerfile:1
# Build context: backend/. Two images come out of this file:
#   docker build --target migrator .   one-off EF Core migration bundle (runs, applies migrations, exits)
#   docker build .                     the API (last stage, the default target)

# ---- Build: restore, publish the API and build the migration bundle ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, from the project files only, so this layer is cached until a dependency changes.
COPY global.json Directory.Build.props ./
COPY .config/dotnet-tools.json .config/
COPY src/TrackManagement.Shared/TrackManagement.Shared.csproj src/TrackManagement.Shared/
COPY src/TrackManagement.Domain/TrackManagement.Domain.csproj src/TrackManagement.Domain/
COPY src/TrackManagement.Application/TrackManagement.Application.csproj src/TrackManagement.Application/
COPY src/TrackManagement.Infrastructure/TrackManagement.Infrastructure.csproj src/TrackManagement.Infrastructure/
COPY src/TrackManagement.Api/TrackManagement.Api.csproj src/TrackManagement.Api/
RUN dotnet restore src/TrackManagement.Api/TrackManagement.Api.csproj && dotnet tool restore

COPY src/ src/
RUN dotnet publish src/TrackManagement.Api/TrackManagement.Api.csproj \
        --configuration Release --no-restore --output /out/api /p:UseAppHost=false
# Building the bundle creates the DbContext through the API's Program, which requires a connection string.
# It isn't used here (nothing connects at build time); the real one is read when the bundle runs.
RUN ConnectionStrings__DefaultConnection="Server=build-time-placeholder" \
    dotnet ef migrations bundle \
        --project src/TrackManagement.Infrastructure --startup-project src/TrackManagement.Api \
        --configuration Release --output /out/efbundle

# ---- Migrator: applies pending migrations, then exits ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS migrator
WORKDIR /app
# The bundle builds the DbContext through the API's Program, which reads appsettings.json from here.
COPY --from=build /out/api/appsettings.json ./
COPY --from=build /out/efbundle ./efbundle
USER $APP_UID
ENTRYPOINT ["./efbundle"]

# ---- API (default target) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=build /out/api ./
# Non-root user built into the .NET 8 images (UID 1654); Kestrel listens on 8080.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TrackManagement.Api.dll"]
