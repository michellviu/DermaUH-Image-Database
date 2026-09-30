# DermaUH Image Database

**DermaUH Image Database** es una plataforma web integral diseñada para el almacenamiento, gestión y anotación de imágenes dermatológicas clínicas. Este sistema facilita el trabajo de especialistas médicos y desarrolladores al ofrecer un entorno seguro para manejar imágenes médicas, gestionar usuarios y solicitudes de descarga, y servir como base fundamental para la investigación médica y el entrenamiento de modelos de Inteligencia Artificial en el campo de la dermatología.

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

## Despliegue con Docker

El repositorio incluye `docker-compose.yml` con PostgreSQL, la API y la aplicación web. Solo se necesita Docker con el complemento Compose v2.

```bash
cp .env.example .env   # ajustar contraseñas, JWT_SECRET_KEY y el administrador inicial
docker compose up -d --build
```

| Servicio   | URL por defecto                 | Descripción                                   |
|------------|---------------------------------|-----------------------------------------------|
| `web`      | http://localhost:5130           | Aplicación Blazor Server                      |
| `api`      | http://localhost:5131           | API REST (Swagger solo en `Development`)      |
| `postgres` | 127.0.0.1:5432                  | Base de datos, publicada solo en el host      |
| `mailpit`  | http://localhost:8025           | Opcional (`--profile mail`): correo de prueba |

- **Migraciones**: se aplican automáticamente al arrancar la API.
- **Primer administrador**: una base de datos nueva no tiene usuarios. Definir `BOOTSTRAP_ADMIN_EMAIL` y `BOOTSTRAP_ADMIN_PASSWORD` en `.env` para que la API cree esa cuenta con rol `Admin` al arrancar (si ya existe, solo le asigna el rol).
- **Persistencia**: volúmenes `postgres_data` (base de datos), `api_images` (archivos de imagen) y `api_keys`/`web_keys` (claves de Data Protection; sin ellas, los enlaces de confirmación y de recuperación de contraseña dejan de ser válidos tras reiniciar).
- **Correo**: sin SMTP configurado los correos solo se registran en el log y los usuarios no pueden confirmar su cuenta. Para pruebas locales: `SMTP_SERVER_ADDRESS=mailpit`, `SMTP_SERVER_PORT=1025` y `docker compose --profile mail up -d`.
- **Producción**: cambiar todos los secretos de `.env`, poner un proxy inverso con HTTPS **con soporte de WebSocket** (Blazor Server lo necesita) delante de `web` y `api`, y ajustar `FRONTEND_BASE_URL` a la URL pública.

Comandos habituales:

```bash
docker compose logs -f api web                                  # registros
docker compose down                                             # detener (conserva los datos)
docker compose exec postgres pg_dump -U root DermaImage > dermauh.sql   # copia de la base de datos
```

> `docker compose down -v` elimina también los volúmenes (base de datos e imágenes).

Si `docker compose build` falla con *timeouts* al restaurar paquetes de NuGet (HTTPS) mientras hay una VPN activa, la causa suele ser la MTU de la red de Docker: ajustar `"mtu"` en `/etc/docker/daemon.json` al valor de la interfaz de la VPN (p. ej. `1420`) y reiniciar Docker.

## Manual de usuario

El manual de usuario está en [`manual-usuario/manual.pdf`](manual-usuario/manual.pdf); su fuente LaTeX es `manual-usuario/manual.tex` y las capturas están en `manual-usuario/figuras/`. Para regenerarlo:

```bash
cd manual-usuario
latexmk -pdf manual.tex
```

## Configuración de Email SMTP

La API usa `MailKit` y configuración tipada en `EmailSettings`.

Variables de entorno soportadas:

- `EmailSettings__SmtpServerAddress` (ej.: `smtp.gmail.com`)
- `EmailSettings__SmtpServerPort` (ej.: `587`)
- `EmailSettings__SmtpUserName` (correo SMTP)
- `EmailSettings__SmtpPassword` (contraseña SMTP)
- `EmailSettings__EnableSSL` (`true` para SSL directo, `false` para STARTTLS)
- `EmailSettings__EmailAddress` (remitente)
- `EmailSettings__EmailAddressDisplay` (nombre visible del remitente)
- `EmailSettings__EnvironmentSubjectPrefix` (prefijo opcional del asunto, ej. `DEV`)

Notas:

- En desarrollo local también se definieron estas variables en `apps/api/src/WebApi.DermaImage/Properties/launchSettings.json`.
- Si usas Gmail, Google suele requerir **App Password** en vez de contraseña normal de cuenta para SMTP.
