# DermaUH Image Database

En este repositorio se estará desarrollando la base de imágenes de DermaUH.

## Requisitos

- .NET 10.0 SDK
- PostgreSQL

## Estructura del proyecto

```
apps/
  api/src/
    Domain.DermaImage/         # Entidades, Enums, Interfaces
    Application.DermaImage/    # Servicios, Managers, DTOs
    Infrastructure.DermaImage/ # DbContext, Repositorios, Configuraciones EF
    WebApi.DermaImage/         # Controllers, Program.cs (startup)
  web/                         # Blazor Server (frontend)
```

## Configuración

1. Configurar la cadena de conexión en `apps/api/src/WebApi.DermaImage/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DermaImage;Username=root;Password=root1234"
  }
}
```

2. Las migraciones pendientes se aplican automáticamente al iniciar la API.

## Migraciones de Base de Datos

El proyecto usa **Entity Framework Core** con PostgreSQL. Las migraciones se ejecutan desde la carpeta `apps/api/src/`.

### Crear una nueva migración

Cuando modifiques entidades, configuraciones de EF o el `DbContext`, genera una nueva migración:

```bash
cd apps/api/src
dotnet ef migrations add <NombreDeLaMigracion> \
  --project Infrastructure.DermaImage \
  --startup-project WebApi.DermaImage
```

**Ejemplo:**

```bash
dotnet ef migrations add InitialCreate \
  --project Infrastructure.DermaImage \
  --startup-project WebApi.DermaImage
```

Esto creará los archivos de migración en `Infrastructure.DermaImage/Migrations/`.

### Aplicar migraciones manualmente

```bash
cd apps/api/src
dotnet ef database update \
  --project Infrastructure.DermaImage \
  --startup-project WebApi.DermaImage
```

### Revertir una migración

```bash
cd apps/api/src
dotnet ef database update <MigracionAnterior> \
  --project Infrastructure.DermaImage \
  --startup-project WebApi.DermaImage
```

### Eliminar la última migración (si no fue aplicada)

```bash
cd apps/api/src
dotnet ef migrations remove \
  --project Infrastructure.DermaImage \
  --startup-project WebApi.DermaImage
```

> **Nota:** Al iniciar la API, las migraciones pendientes se aplican automáticamente mediante `db.Database.MigrateAsync()` en `Program.cs`.

## Ejecutar

```bash
# API
cd apps/api/src
dotnet run --project WebApi.DermaImage

# Web (Blazor)
cd apps/web
dotnet run
```
