FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY global.json ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY SendHub.slnx ./

COPY src/SendHub/SendHub.csproj                                                 src/SendHub/
COPY src/SendHub.Features/SendHub.Features.csproj                               src/SendHub.Features/
COPY src/SendHub.Infrastructure/SendHub.Infrastructure.csproj                   src/SendHub.Infrastructure/
COPY src/SendHub.Web/SendHub.Web.csproj                                         src/SendHub.Web/
COPY src/Aspire/SendHub.ServiceDefaults/SendHub.ServiceDefaults.csproj          src/Aspire/SendHub.ServiceDefaults/

RUN dotnet restore src/SendHub.Web/SendHub.Web.csproj


FROM restore AS build

COPY src/SendHub/                              src/SendHub/
COPY src/SendHub.Features/                     src/SendHub.Features/
COPY src/SendHub.Infrastructure/               src/SendHub.Infrastructure/
COPY src/SendHub.Web/                          src/SendHub.Web/
COPY src/Aspire/SendHub.ServiceDefaults/       src/Aspire/SendHub.ServiceDefaults/

RUN dotnet publish src/SendHub.Web/SendHub.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN useradd --system --no-create-home --shell /usr/sbin/nologin sendhub

WORKDIR /app

RUN mkdir -p /data/scan /data/db \
    && chown -R sendhub:sendhub /data

COPY --from=build /app/publish ./

USER sendhub

ENV SendHub__WatchFolder=/data/scan
ENV SendHub__DestinationFolder=/data/scan/Processed
ENV SendHub__Database__Path=/data/db/sendhub.db
ENV DOTNET_ENVIRONMENT=Production

EXPOSE 8080

VOLUME ["/data/scan", "/data/db"]

ENTRYPOINT ["dotnet", "SendHub.Web.dll"]
