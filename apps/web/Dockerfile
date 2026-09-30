# Build context must be the repository root (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Node.js is required at build time: Web.DermaImage.csproj runs
# `npm run css:build` (Tailwind) as an MSBuild target before compiling.
RUN apt-get update \
    && apt-get install -y --no-install-recommends nodejs npm \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

COPY apps/web/Web.DermaImage.csproj .
# The SDK only restores the package that ships _framework/blazor.web.js when it
# sees .razor files, which are not copied yet at this point; without this flag
# the published app renders a blank page.
RUN dotnet restore Web.DermaImage.csproj -p:RequiresAspNetWebAssets=true

COPY apps/web/package.json apps/web/package-lock.json* ./
RUN npm install --no-audit --no-fund

COPY apps/web/ .
RUN dotnet publish Web.DermaImage.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Mount point for the Data Protection key volume; must belong to the non-root
# "app" user shipped with the base image, otherwise Docker creates it as root.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /home/app/.aspnet

COPY --from=build /app/publish .

USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.DermaImage.dll"]
