# Guía de Uso y Desarrollo (DoctorWare API)

Esta guía resume cómo configurar, ejecutar y consumir la API; además explica los patrones base (repositorios/servicios genéricos) para evitar código repetido.

## Requisitos
- .NET SDK 10 (TargetFramework `net10.0`).
- PostgreSQL 14+.
- Variables/`appsettings` configuradas (ver más abajo).

## Ejecución rápida
1) Configura la cadena de conexión en `BackendDoctorWare/DoctorWare/appsettings.Development.json` (`ConnectionStrings:ConexionPredeterminada`).
2) Ejecuta en el puerto 3000:

```
dotnet run --project BackendDoctorWare/DoctorWare/DoctorWare.Api.csproj --urls http://localhost:3000
```

3) Endpoints útiles:
- Swagger UI: `http://localhost:3000/swagger`
- Health: `http://localhost:3000/health`
- OpenAPI JSON: `http://localhost:3000/swagger/v1/swagger.json`

Notas:
- En Development, al iniciar se verifica la DB y se ejecutan scripts de `DoctorWare/Scripts` en orden alfabético (idempotente). Ver Data/EjecutorScriptsSQL.

## Configuración (appsettings / variables)
- Archivo: `BackendDoctorWare/DoctorWare/appsettings.Development.json`.
- Claves relevantes:
  - `ConnectionStrings:ConexionPredeterminada`
  - `Cors:AllowedOrigins` (ej.: `http://localhost:4200`)
  - `Jwt:{ Secret, Issuer, Audience, AccessTokenMinutes, RefreshTokenMinutes }`
  - `Email:Smtp:{ Host, Port, EnableSsl, User, Password, FromEmail, FromName }`
  - `EmailConfirmation:{ TokenMinutes, FrontendConfirmUrl, BackendConfirmRedirectUrl, ResendCooldownSeconds }`

Variables de entorno equivalentes (ejemplos):
- `ConnectionStrings__ConexionPredeterminada` | `Jwt__Secret` | `Cors__AllowedOrigins:0`

## Errores y formato de respuestas
- Middleware global estandariza errores a `{ success, data, message, errorCode }`.
- 401/403 personalizados por eventos de JwtBearer.
- 404/405 también responden JSON uniforme.

## Autenticación y tokens
- Endpoints de Auth: `/api/auth/*`.
- Flujos: registro → confirmación de email → login → refresh → me.
- El front debe enviar `Authorization: Bearer {token}` en endpoints protegidos.

## SQL Scripts (migraciones simples)
- Carpeta: `BackendDoctorWare/DoctorWare/Scripts`.
- Al iniciar en Development, `Data/EjecutorScriptsSQL` crea `schema_migrations` y ejecuta `*.sql` en orden alfabético; cada archivo se registra y no se vuelve a ejecutar.
- Para ejecutar uno puntual: inyectar `EjecutorScriptsSQL` y llamar `EjecutarScriptEspecificoAsync(nombre)`.

## CORS y Swagger
- CORS lee `Cors:AllowedOrigins` y registra la política `PermitirAngular`.
- Swagger disponible siempre; UI en `/swagger`.

## Consumo de la API (resumen)
- Contrato completo: `BackendDoctorWare/API_CONTRACT.md`.
- Base URL (dev): `http://localhost:3000/api`.
- Headers: `Content-Type: application/json`, `Accept: application/json`, `Authorization: Bearer {token}` (cuando aplique).

Ejemplos rápidos (cURL):

Login
```
curl -X POST http://localhost:3000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"123456"}'
```

Listar pacientes
```
curl "http://localhost:3000/api/patients?name=juan" -H "Authorization: Bearer $TOKEN"
```

Crear turno
```
curl -X POST http://localhost:3000/api/appointments \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{
    "patientId":"100","professionalId":"45","date":"2025-11-15T00:00:00Z",
    "startTime":"09:00","duration":30,"type":"first_visit",
    "reason":"Control anual","notes":"Traer estudios"
  }'
```

## Arquitectura y patrones (DRY)

### Conexión a DB
- `Data/Interfaces/IDbConnectionFactory.cs` + `Data/Implementation/DbConnectionFactory.cs`: centralizan la creación de conexiones (Npgsql) con DI.

### Repositorio base
- Archivo: `Repositories/Implementation/BaseRepository.cs`.
- Propósito: evitar duplicar CRUD y paginación con Dapper.
- Cómo usar:
  1. Crea una clase `FooRepository : BaseRepository<FOO, int>`.
  2. Implementa:
     - `protected override string Table => "public.\"FOO\"";`
     - `protected override string Key => "\"ID_FOO\"";` (opcional si no es `id`)
     - `protected override string SelectColumns => "...";`
     - `protected override object ToDb(FOO e) => new { ... };`
     - `protected override string InsertSql => "insert into ... returning ...";`
     - `protected override string UpdateSql => "update ... where ...";`
  3. Inyéctalo vía DI y úsalo desde un servicio.

### Servicio base
- Archivo: `Services/Implementation/BaseService.cs` + contrato `Services/Interfaces/IService.cs`.
- Propósito: encapsular flujo Create/Update/Delete/Get con hooks de validación.
- Cómo usar:
  1. Crea `FooService : BaseService<FOO, int>` y recibe `IRepository<FOO, int>` en el constructor.
  2. Sobre-escribe `ValidateAsync/BeforeSaveAsync/AfterSaveAsync` si aplica.
  3. Registra el servicio en DI y úsalo en tu controlador.

### Respuestas y errores homogéneos
- `DTOs/Response/ApiResponse.cs` y `Middleware/ErrorHandlingMiddleware.cs` estandarizan el formato.

### Mapeos y helpers
- `Services/Implementation/Helpers/AppointmentMappingHelper.cs`: mapea estados/tipos DB↔Front y formatea horarios.

## Añadir una nueva entidad (paso a paso)
1) SQL: crea tabla y claves en un nuevo `Scripts/Script_XXX_*.sql`.
2) Modelo/DTOs: define DTOs de request/response bajo `DTOs/`.
3) Repositorio: hereda de `BaseRepository<T,TKey>` y completa `Table/Key/SelectColumns/ToDb/InsertSql/UpdateSql`.
4) Servicio: si es CRUD simple, hereda de `BaseService<T,TKey>` y agrega reglas; si es caso de uso complejo, implementa un servicio específico (como `PatientsService`).
5) Controller: crea controlador bajo `Controllers/` con `[ApiController]` y `[Route("api/[controller]")]`.
6) DI: registra repositorio/servicio en `Program.cs`.
7) Swagger: verifica tipos y ejemplos; compila.

## Solución de problemas
- Error al compilar por SDK: verifica que tienes .NET 10 (`dotnet --list-sdks`).
- Conexión a DB: revisa `ConnectionStrings:ConexionPredeterminada`; prueba con `psql`/`SELECT 1`.
- Scripts SQL no corren: confirma carpeta `DoctorWare/Scripts` y permisos del usuario DB.
- CORS: añade el origen del front en `Cors:AllowedOrigins`.

## Recursos
- Contrato detallado de endpoints: `BackendDoctorWare/API_CONTRACT.md`.
- README de alto nivel: `BackendDoctorWare/README.md`.

