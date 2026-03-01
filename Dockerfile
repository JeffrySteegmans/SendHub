FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY global.json ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY SendHub.slnx ./

COPY src/SendHub/SendHub.csproj                               src/SendHub/
COPY src/SendHub.Features/SendHub.Features.csproj             src/SendHub.Features/
COPY src/SendHub.Infrastructure/SendHub.Infrastructure.csproj src/SendHub.Infrastructure/
COPY src/SendHub.Daemon/SendHub.Daemon.csproj                 src/SendHub.Daemon/

RUN dotnet restore src/SendHub.Daemon/SendHub.Daemon.csproj


FROM restore AS build

COPY src/SendHub/                src/SendHub/
COPY src/SendHub.Features/       src/SendHub.Features/
COPY src/SendHub.Infrastructure/ src/SendHub.Infrastructure/
COPY src/SendHub.Daemon/         src/SendHub.Daemon/

RUN dotnet publish src/SendHub.Daemon/SendHub.Daemon.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish


FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

RUN adduser --disabled-password --gecos "" sendhub

WORKDIR /app

RUN mkdir -p /data/scan /data/tracking \
    && chown -R sendhub:sendhub /data

COPY --from=build /app/publish ./

USER sendhub

ENV SendHub__WatchFolder=/data/scan
ENV SendHub__DestinationFolder=/data/scan/Processed
ENV SendHub__Tracking__FilePath=/data/tracking/tracking.json
ENV DOTNET_ENVIRONMENT=Production

VOLUME ["/data/scan", "/data/tracking"]

ENTRYPOINT ["dotnet", "SendHub.Daemon.dll"]
