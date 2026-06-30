# Documentación de Pruebas: DermaUH Image Database

En respuesta a la necesidad de validar funcionalmente y medir el rendimiento del sistema **DermaUH Image Database**, se ha implementado una arquitectura de pruebas de tres niveles utilizando tecnologías estándar del ecosistema .NET.

## 1. Pruebas Unitarias (`Tests.Unit.DermaImage`)

**Objetivo:** Validar la exactitud de las reglas de negocio y el comportamiento interno de los componentes aislados de sus dependencias (bases de datos, red).

**Tecnologías:**
- **xUnit:** Framework de ejecución de pruebas.
- **Moq:** Para la creación de objetos simulados (mocks) que reemplazan la capa de acceso a datos.
- **FluentAssertions:** Para escribir aserciones legibles y descriptivas.

**Escenarios Cubiertos:**
- Validación estricta de metadatos de imágenes (ej. coherencia entre `DiagnosisCategory` e `InjuryType`).
- Lógica de orquestación en la capa de Aplicación (`DermaImgManager`).
- Generación y asignación de identificadores únicos (`PublicId`) antes de la persistencia.

---

## 2. Pruebas de Integración (`Tests.Integration.DermaImage`)

**Objetivo:** Asegurar que los diferentes subsistemas (Controladores Web API, Lógica de Aplicación, Entity Framework Core) funcionen correctamente en conjunto.

**Tecnologías:**
- **Microsoft.AspNetCore.Mvc.Testing:** Permite hospedar la API en memoria (TestServer) para realizar peticiones HTTP de prueba sin necesidad de desplegar la aplicación real.
- **EF Core InMemory:** Sustituye la base de datos PostgreSQL real por una instancia en memoria volátil, garantizando que las pruebas sean rápidas, aisladas y no muten datos reales.

**Escenarios Cubiertos:**
- Peticiones HTTP a endpoints protegidos y públicos.
- Restricciones de paginación para usuarios anónimos (límite de 10 elementos).
- Recuperación exitosa de imágenes sembradas en la base de datos de prueba y respuestas `404 Not Found` ante accesos inválidos.

---

## 3. Pruebas de Carga y Rendimiento (`Tests.Load.DermaImage`)

**Objetivo:** Evaluar la resiliencia del sistema ante escenarios de alta concurrencia y tráfico pesado, determinando los límites operativos antes de experimentar latencia degradada o errores de conexión.

**Tecnologías:**
- **NBomber:** Framework open-source diseñado específicamente para pruebas de estrés y simulaciones de carga en aplicaciones .NET.

**Escenarios Cubiertos (Simulación):**
- **Navegación de galería pública:** Inyección gradual (Ramp-up) hasta 50 usuarios simultáneos solicitando paginación de imágenes.
- **Descarga / Previsualización de imágenes:** Inyección agresiva de hasta 100 usuarios simultáneos solicitando recursos estáticos y metadatos de imágenes en paralelo.

**Salida:** NBomber genera un reporte detallado en HTML/Markdown (`LoadTestReports/`) con métricas críticas como:
- *RPS (Requests Per Second)*.
- *Latencia Media, Min, Max y percentiles (p95, p99)*.
- *Tasa de éxito vs tasa de fallos*.

---

## Instrucciones de Ejecución

Para ejecutar toda la suite de pruebas funcionales (Unitarias e Integración) desde la terminal:

```bash
cd /apps/api/src
dotnet test
```

Para ejecutar las pruebas de carga, asegúrese de que el API esté en ejecución en su puerto respectivo (ej. `https://localhost:7262`) y luego ejecute:

```bash
cd /apps/api/src/Tests.Load.DermaImage
dotnet run -c Release
```
*(Los reportes de carga se generarán en la carpeta `LoadTestReports` dentro del proyecto).*
