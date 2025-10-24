# BackendDoctorWare

Backend para el sistema de gestión de consultorios médicos DoctorWare.

## Actualización Rápida (compatibilidad con el frontend)

- Respuestas JSON en camelCase habilitadas globalmente.
- Tokens JWT incluyen claims `sub`, `email`, `name` (si está disponible) y `role`.
- Endpoints de autenticación (forma plana, compatibles con el frontend Angular):
  - POST `/api/auth/register` → `{ token, refreshToken, user, expiresIn }`
  - POST `/api/auth/login` → `{ token, refreshToken, user, expiresIn }`
  - POST `/api/auth/refresh` → `{ token, refreshToken, user, expiresIn }`
  - GET  `/api/auth/me` → devuelve el `user` autenticado
  - GET  `/api/auth/confirm-email?uid={id}&token={token}` → confirma email y redirige según config
- Para alinear con el front por defecto, puedes ejecutar la API en `http://localhost:3000`:

```
dotnet run --project DoctorWare/DoctorWare.Api.csproj --urls http://localhost:3000
```

## Estructura del Proyecto

- `DoctorWare/`: Proyecto ASP.NET Core
  - `Controllers/`: Controladores con endpoints HTTP
  - `Services/`: Lógica de negocio
  - `Repositories/`: Acceso a datos (Dapper)
  - `Data/`: Conexión a BD, salud y ejecución de scripts
  - `Models/`: Entidades de dominio (mapeos a tablas)
  - `DTOs/`: Contratos de entrada/salida
  - `Mappers/`: Traducción de modelos a DTOs
  - `Helpers/`: Utilidades transversales (claims, etc.)
  - `Middleware/`: Manejo global de errores, logging, etc.
  - `Program.cs`: Configuración de servicios y pipeline

## Puesta en Marcha

1) Configura la cadena de conexión en `DoctorWare/appsettings.Development.json` → `ConnectionStrings:ConexionPredeterminada`.
2) En Development, al iniciar se ejecutan los scripts SQL de `DoctorWare/Scripts` (idempotentes) y se verifica la conexión.
3) Swagger disponible en `https://localhost:5001/swagger` o según el puerto configurado.

### Email de confirmación (dev)

- Configura SMTP en `DoctorWare/appsettings.Development.json` sección `Email:Smtp`.
- Configura `EmailConfirmation`:
  - `TokenMinutes`: vigencia del token.
  - `FrontendConfirmUrl`: opcional, URL del front para manejar la confirmación (se agregan `uid` y `token`).
  - `BackendConfirmRedirectUrl`: adonde redirige el endpoint si se invoca directamente.

## Puertos y Ejecución Local

- Valores por defecto:
  - HTTP: `http://localhost:5000`
  - HTTPS: `https://localhost:5001`
- Variable `BaseUrl` en `appsettings*.json` apunta a `http://localhost:5000`.
- Forzar puertos (ejemplos):
  - PowerShell: `setx ASPNETCORE_URLS "https://localhost:5001;http://localhost:5000"`
  - Bash: `export ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"`

## Autenticación y Swagger

- JWT configurado en `DoctorWare/appsettings*.json` sección `Jwt` (Secret, Issuer, Audience, tiempos).
- Endpoints de autenticación (compatibles con el frontend):
  - POST `/api/auth/register` → `{ token, refreshToken, user, expiresIn }`
  - POST `/api/auth/login` → `{ token, refreshToken, user, expiresIn }`
  - POST `/api/auth/refresh` → `{ token, refreshToken, user, expiresIn }`
  - GET  `/api/auth/me` → Perfil del usuario autenticado (requiere `Authorization: Bearer {token}`)
- Serialización JSON: camelCase global.

### ¿Qué es `/api/auth/me`?

Devuelve el perfil del usuario autenticado leyendo el JWT del header `Authorization: Bearer {token}`.
- Método: GET `/api/auth/me`
- Body: no requerido
- Uso típico: recuperar sesión al iniciar la app, validar el token y obtener datos del usuario.

### Swagger UI

- UI: `https://localhost:5001/swagger`
- JSON: `https://localhost:5001/swagger/v1/swagger.json`

## Pruebas Rápidas (REST Client)

Archivo `DoctorWare/DoctorWare.http` con ejemplos de `login`, `refresh` y `me` (usar GET en `me`).

## Uso desde Angular (ejemplos)

- Base URL sugerida para integrar con este backend:

```ts
// FrontEndDoctorWare/src/environments/environment.ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:3000/api',
  jwtStorageKey: 'doctorware_token',
  appName: 'DoctorWare',
  logLevel: 'debug'
};
```

- Servicio de autenticación (forma plana, sin ApiResponse):

```ts
// src/app/core/services/auth.service.ts (fragmento)
interface AuthResult {
  token: string;
  refreshToken: string;
  user: any; // coincide con el 'User' que espera el front
  expiresIn: number;
}

login(email: string, password: string) {
  return this.http.post<AuthResult>(`${this.base}/auth/login`, { email, password });
}

refresh(refreshToken: string) {
  return this.http.post<AuthResult>(`${this.base}/auth/refresh`, { refreshToken });
}

me() {
  return this.http.get<any>(`${this.base}/auth/me`);
}
```

## CORS

- Orígenes permitidos en Development (`DoctorWare/appsettings.Development.json`):
  - `http://localhost:4200`, `http://localhost:4201`, `http://localhost:3000`

## Buenas Prácticas Aplicadas

- Middleware global de errores (respuestas homogéneas en producción y detalle en Development).
- Health checks: `GET /health` con verificación de DB.
- Ejecución de scripts SQL idempotente (control en tabla `schema_migrations`).
- Nullability y analizadores habilitados.
- Logging estructurado con Serilog.

## Notas

- Si al compilar aparece “archivo en uso”, detén instancias previas y ejecuta `dotnet clean` antes de compilar.
