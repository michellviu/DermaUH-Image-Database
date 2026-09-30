# Build context must be the repository root (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, using only the .csproj files, so dependency layers are
# cached and skipped on rebuilds that only touch source code.
COPY apps/api/src/Domain.DermaImage/Domain.DermaImage.csproj Domain.DermaImage/
COPY apps/api/src/Application.DermaImage/Application.DermaImage.csproj Application.DermaImage/
COPY apps/api/src/Infrastructure.DermaImage/Infrastructure.DermaImage.csproj Infrastructure.DermaImage/
COPY apps/api/src/WebApi.DermaImage/WebApi.DermaImage.csproj WebApi.DermaImage/
RUN dotnet restore WebApi.DermaImage/WebApi.DermaImage.csproj

COPY apps/api/src/Domain.DermaImage/ Domain.DermaImage/
COPY apps/api/src/Application.DermaImage/ Application.DermaImage/
COPY apps/api/src/Infrastructure.DermaImage/ Infrastructure.DermaImage/
COPY apps/api/src/WebApi.DermaImage/ WebApi.DermaImage/

RUN dotnet publish WebApi.DermaImage/WebApi.DermaImage.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Mount points for the named volumes must exist and belong to the non-root
# "app" user shipped with the base image, otherwise Docker creates them as root.
RUN mkdir -p /app/ImageStorage /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /app/ImageStorage /home/app/.aspnet

COPY --from=build /app/publish .

USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080 \
    ImageStorage__RootPath=/app/ImageStorage
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.DermaImage.dll"]
