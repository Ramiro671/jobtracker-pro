# ─── Stage 1: Build ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY JobTrackerPro.slnx .
COPY src/JobTrackerPro.Domain/JobTrackerPro.Domain.csproj src/JobTrackerPro.Domain/
COPY src/JobTrackerPro.Application/JobTrackerPro.Application.csproj src/JobTrackerPro.Application/
COPY src/JobTrackerPro.Infrastructure/JobTrackerPro.Infrastructure.csproj src/JobTrackerPro.Infrastructure/
COPY src/JobTrackerPro.Api/JobTrackerPro.Api.csproj src/JobTrackerPro.Api/

RUN dotnet restore src/JobTrackerPro.Api/JobTrackerPro.Api.csproj

COPY src/ src/
RUN dotnet publish src/JobTrackerPro.Api/JobTrackerPro.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "JobTrackerPro.Api.dll"]
