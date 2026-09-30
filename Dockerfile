ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0-noble
ARG BUILD_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0

# -------------------------
# BUILD STAGE
# -------------------------
FROM ${BUILD_IMAGE} AS build
WORKDIR /src

ARG CI_COMMIT_SHORT_SHA="00000000"
ARG CI_JOB_STARTED_AT="now"

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

# -------------------------
# CERTIFICATES (скачиваем внутри контейнера)
# -------------------------
RUN apt-get update && apt-get install -y --no-install-recommends \
        wget \
        ca-certificates \
    && mkdir -p /usr/local/share/ca-certificates/Yandex \
    && wget --max-redirect=0 "https://storage.yandexcloud.net/cloud-certs/RootCA.pem" \
        -O /usr/local/share/ca-certificates/Yandex/RootCA.crt \
    && wget --max-redirect=0 "https://storage.yandexcloud.net/cloud-certs/IntermediateCA.pem" \
        -O /usr/local/share/ca-certificates/Yandex/IntermediateCA.crt \
    && chmod 644 /usr/local/share/ca-certificates/Yandex/*.crt \
    && update-ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# -------------------------
# RESTORE
# -------------------------
COPY nuget.config .
COPY Directory.Packages.props .

COPY src/IRM.Settlements.Application/*.csproj         IRM.Settlements.Application/
COPY src/IRM.Settlements.Domain/*.csproj              IRM.Settlements.Domain/
COPY src/IRM.Settlements.Infrastructure/*.csproj      IRM.Settlements.Infrastructure/
COPY src/IRM.Settlements.Api/*.csproj                 IRM.Settlements.Api/

RUN dotnet restore IRM.Settlements.Api/IRM.Settlements.Api.csproj

# -------------------------
# BUILD
# -------------------------
COPY src/ .

# Pre-generate Wolverine handler / endpoint adapter code into
# Application/Internal/Generated/ so the production image ships static C#
# instead of compiling at boot. Pair with TypeLoadMode.Static in production.
RUN dotnet run --no-restore \
    --project IRM.Settlements.Api/IRM.Settlements.Api.csproj \
    -- codegen write

RUN dotnet publish IRM.Settlements.Api/IRM.Settlements.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# -------------------------
# VERSION + SENTRY
# -------------------------
COPY VERSION .
RUN RELEASE_VERSION="$(cat ./VERSION)" \
    && SENTRY_RELEASE="${RELEASE_VERSION}-${CI_COMMIT_SHORT_SHA}-${CI_JOB_STARTED_AT}" \
    && sed -i "s#\"Version\": \".*\"#\"Version\": \"${SENTRY_RELEASE}\"#g" /app/publish/appsettings.json \
    && sed -i "s#\"Release\": \".*\"#\"Release\": \"${SENTRY_RELEASE}\"#g" /app/publish/appsettings.json

# -------------------------
# RUNTIME STAGE
# -------------------------
FROM ${RUNTIME_IMAGE} AS final
WORKDIR /app

# копируем только сертификаты
COPY --from=build /etc/ssl/certs /etc/ssl/certs
COPY --from=build /usr/local/share/ca-certificates /usr/local/share/ca-certificates

# приложение
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER app
ENTRYPOINT ["dotnet", "IRM.Settlements.Api.dll"]
