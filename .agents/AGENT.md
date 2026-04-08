# AGENT.md

Guia de contexto para agentes de IA que implementen nuevos features en DermaUH Image Database.

## 1) Objetivo del proyecto

Plataforma para gestionar imagenes dermatologicas con:

- API en ASP.NET Core (arquitectura por capas).
- Frontend en Blazor Server interactivo.
- Persistencia en PostgreSQL con Entity Framework Core.
- Autenticacion con ASP.NET Identity + JWT.

## 2) Arquitectura general

Estructura principal:

- `apps/api/src/Domain.DermaImage`: entidades, enums e interfaces base.
- `apps/api/src/Application.DermaImage`: DTOs, managers, servicios y reglas de negocio.
- `apps/api/src/Infrastructure.DermaImage`: DbContext, repositorios, configuracion EF, servicios de infraestructura.
- `apps/api/src/WebApi.DermaImage`: controladores, middleware, startup y endpoints HTTP.
- `apps/web`: frontend Blazor Server (`InteractiveServer`) con autenticacion y consumo de API.

Regla de dependencia:

- Domain no depende de otras capas.
- Application depende de Domain.
- Infrastructure implementa contratos de Domain/Application.
- WebApi compone y expone los casos de uso.

## 3) Backend (normas para features)

### 3.1 Seguridad y autorizacion

- La API usa politica por defecto segura: todo endpoint requiere usuario autenticado.
- Solo usar `[AllowAnonymous]` cuando sea estrictamente necesario.
- Para endpoints protegidos por rol, usar `[Authorize(Roles = "...")]` o validacion equivalente coherente.
- Mantener consistencia de roles existentes: `Admin`, `Contributor`, `Reviewer`, `Viewer`.

Matriz actual relevante:

- `UsersController`: solo `Admin`.
- `ImagesController`: lectura publica (solo imagenes publicas), escritura para `Admin` y `Contributor`.
- `InstitutionsController`: operaciones criticas restringidas a admin; solicitudes de asociacion para autenticados.
- `AuthController`: login/registro/confirmacion/reset anonimos; perfil y cambio de contrasena autenticados.

### 3.2 Convenciones de endpoints

- Usar DTOs para entrada/salida; no exponer entidades de dominio directamente.
- Mantener paginacion con `PagedResponse<T>` en listados.
- Incluir `CancellationToken` en operaciones async de controladores/managers/repositorios.
- Devolver codigos HTTP semanticos (`200`, `201`, `204`, `400`, `401`, `403`, `404`, `500`).
- Preferir mensajes de error claros en espanol para respuestas funcionales al cliente.

### 3.3 Validacion

- Respetar validacion automatica de `ModelState` (ya configurada en `Program.cs`).
- Para reglas de negocio adicionales, seguir el patron de validadores de Application (ejemplo: `DermaImgValidationRules`).
- Si hay errores de negocio, devolver `ValidationProblem(...)` o `BadRequest(...)` estructurado.

### 3.4 Datos e integridad

- En imagenes, la `InstitutionId` debe derivarse del contribuidor en backend, no desde input libre de UI.
- En features de membresia institucional, respetar flujo de solicitudes y revision (inbox del responsable).
- Evitar consultas EF en paralelo sobre el mismo `DbContext` scoped.
- En filtros de fecha para PostgreSQL `timestamp with time zone`, usar `DateTime` en UTC.

### 3.5 Persistencia y migraciones

- Al cambiar entidades/configuracion EF, crear migracion en `Infrastructure.DermaImage/Migrations`.
- Startup aplica migraciones pendientes automaticamente.
- Comando base:
  - `cd apps/api/src`
  - `dotnet ef migrations add <Nombre> --project Infrastructure.DermaImage --startup-project WebApi.DermaImage`

## 4) Frontend (normas para features)

### 4.1 Stack y estructura

- Frontend en Blazor Server (`apps/web`) con componentes Razor.
- Paginas bajo `Components/Pages/*`.
- Componentes compartidos bajo `Components/Shared/*`.
- Servicios de cliente/autenticacion bajo `Services/*`.
- Para paginas de mediana/alta complejidad, separar markup y logica con patron `*.razor` + `*.razor.cs` (code-behind).
- Cuando haya formularios extensos reutilizables, extraer secciones comunes a componentes compartidos en `Components/Shared/*`.

Convenciones actuales relevantes:

- Flujo de crear/editar imagen reutiliza `Components/Shared/ImageClinicalMetadataForm.razor` para metadatos clinicos.
- `AuthService` esta organizado en parciales (`AuthService.cs`, `AuthService.Authentication.cs`, `AuthService.ProfileMembership.cs`, `AuthService.Errors.cs`) para mejorar mantenibilidad sin cambiar su API publica.

### 4.2 Autenticacion y sesion

- `AuthService` centraliza login, registro y gestion de token en `localStorage`.
- `AuthenticatedHttpClientHandler` adjunta JWT al `HttpClient` para endpoints protegidos.
- Respetar flujos actuales de perfil, cambio de contrasena y asociacion institucional.

### 4.3 Permisos en UI

- Mantener restricciones por rol tanto en rutas como en acciones visibles.
- Funcionalidades de administracion de usuarios deben mantenerse solo para `Admin`.
- Usuarios normales deben gestionar solo su propio perfil y operaciones personales.

### 4.4 Manejo de errores y UX

- Mostrar errores del backend de forma legible para usuarios (preferentemente en espanol).
- Reutilizar parser de errores de validacion (`ApiValidationMessageParser`) cuando aplique.
- Evitar mostrar JSON crudo en pantalla.

### 4.5 Estilos y assets

- Se usa Tailwind para pipeline CSS y tambien estilos Razor/CSS por componente.
- Si se agregan utilidades de estilos o scripts TS, mantener scripts en `apps/web/package.json` consistentes.

## 5) Flujo recomendado para implementar un feature

1. Definir caso de uso y permisos (quien puede leer/escribir).
2. Ajustar DTOs y contratos en Application/Domain.
3. Implementar logica en manager/servicio y repositorio si aplica.
4. Exponer endpoint en WebApi con validaciones y respuestas HTTP correctas.
5. Implementar UI en Blazor con guardas de autorizacion y mensajes amigables.
6. Verificar impacto en datos/migraciones.
7. Probar flujo end-to-end (API + Web).

## 6) Checklist de calidad antes de cerrar cambios

- El feature respeta arquitectura por capas y no rompe dependencias.
- Se aplican reglas de autorizacion correctas (backend y frontend).
- Validaciones funcionales y de modelo cubiertas.
- Mensajes al usuario en espanol, claros y sin exponer detalles internos.
- No se introducen consultas EF paralelas sobre el mismo contexto.
- Si hay cambios de modelo, migracion creada y validada.
- Funciona localmente con `dotnet run` en API y Web.

## 7) Comandos utiles

API:

- `cd apps/api/src`
- `dotnet run --project WebApi.DermaImage`

Web:

- `cd apps/web`
- `dotnet run`
- `npm run css:watch` (durante desarrollo de estilos)

## 8) Notas de consistencia

- Mantener naming y patrones ya presentes en el repositorio.
- Evitar refactors amplios no solicitados cuando el feature es acotado.
- Priorizar cambios pequenos, comprobables y compatibles con el comportamiento actual.
