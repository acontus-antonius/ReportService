FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        chromium \
        fontconfig \
        fonts-dejavu-core \
        fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium \
    Puppeteer__ExecutablePath=/usr/bin/chromium

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ReportService.Shared/ReportService.Shared.csproj ReportService.Shared/
COPY ReportService.Api/ReportService.Api.csproj ReportService.Api/
RUN dotnet restore ReportService.Api/ReportService.Api.csproj

COPY ReportService.Shared/ ReportService.Shared/
COPY ReportService.Api/ ReportService.Api/
WORKDIR /src/ReportService.Api
RUN dotnet publish ReportService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReportService.Api.dll"]
