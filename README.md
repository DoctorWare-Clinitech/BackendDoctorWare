# BackendDoctorWare

Backend para el sistema de gestión de consultorios médicos DoctorWare.

## Descripción General

Este repositorio contiene el código fuente del backend para DoctorWare, una aplicación diseñada para la administración de consultorios médicos. El sistema está construido con una arquitectura robusta utilizando .NET y PostgreSQL, y está diseñado para ser extensible y mantenible.

## Stack Tecnológico

*   **Lenguaje:** C#
*   **Framework:** ASP.NET Core
*   **Base de Datos:** PostgreSQL
*   **Acceso a Datos:** Dapper

## Estructura del Proyecto

La solución está organizada siguiendo una arquitectura limpia y modular:

*   `DoctorWare/`: Directorio principal del proyecto de ASP.NET Core.
    *   `Controllers/`: Controladores de la API que exponen los endpoints.
    *   `Services/`: Capa de servicio que contiene la lógica de negocio principal.
    *   `Repositories/`: Capa de repositorio que abstrae el acceso a la base de datos.
    *   `Data/`: Componentes de bajo nivel para la conexión a la base de datos y ejecución de scripts.
    *   `Models/`: Entidades del dominio de la aplicación.
    *   `DTOs/`: Objetos para la transferencia de datos entre las capas y la API.
    *   `Scripts/`: Scripts SQL para la inicialización y migración del esquema de la base de datos.
    *   `Program.cs`: Punto de entrada de la aplicación.
*   `ER_DOCTORWARE.drawio.xml`: Diagrama Entidad-Relación que documenta el modelo de datos.

## Puesta en Marcha

1.  Configurar la cadena de conexión a la base de datos PostgreSQL en el archivo `appsettings.json`.
2.  Al iniciar la aplicación, se ejecutarán automáticamente los scripts de la base de datos para crear o actualizar el esquema.
3.  La API estará disponible en la URL configurada en `Properties/launchSettings.json`.

## Puertos y ejecución local

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- `BaseUrl` en `appsettings.json` y `appsettings.Development.json` apunta a `http://localhost:5000`.

Ejecución con perfiles:
- `dotnet run --launch-profile http`
- `dotnet run --launch-profile https`

Forzar puertos fuera de perfiles (por ejemplo en producción o hosting simple):
- Windows (PowerShell): `setx ASPNETCORE_URLS "https://localhost:5001;http://localhost:5000"`
- Linux/macOS (bash): `export ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"`

## Prácticas aplicadas

- Middleware global de errores: respuestas homogéneas (ApiResponse) y trazas solo en Development.
- Validación de modelo estandarizada: 400 con lista de errores por campo.
- Health checks: endpoint `GET /health` con verificación de DB (`SELECT 1`).
- Ejecución de scripts SQL idempotente: crea `schema_migrations` y registra scripts ya ejecutados.
- Nullability habilitado (`<Nullable>enable</Nullable>`) y analizadores (`AnalysisLevel=latest`).
- EditorConfig para codificación UTF‑8 y estilo C# consistente.
