# DoctorWare API (Backend)

Backend ASP.NET Core + Dapper + PostgreSQL.

## Cómo ejecutar
- Configura la base de datos: `BackendDoctorWare/DoctorWare/appsettings.Development.json` → `ConnectionStrings:ConexionPredeterminada`.
- Ejecuta en `http://localhost:3000`:

```
dotnet run --project BackendDoctorWare/DoctorWare/DoctorWare.Api.csproj --urls http://localhost:3000
```

- Swagger: `http://localhost:3000/swagger`
- Health: `http://localhost:3000/health`

## APIs disponibles

Autenticación
- POST `/api/auth/register`
  - Body JSON: `{ nombre, apellido, email, password, telefono?, nroDocumento, tipoDocumentoCodigo, genero }`
  - Respuesta: `201 { message, requiresEmailConfirmation: true }`
  - Campos requeridos: `nombre`, `apellido`, `email` (formato email), `password` (>= 6), `nroDocumento`, `tipoDocumentoCodigo` (p.ej. "DNI"), `genero`.
  - Ejemplo request:
    ```json
    {
      "nombre": "Ana",
      "apellido": "Pérez",
      "email": "ana@example.com",
      "password": "Secreta1!",
      "telefono": "",
      "nroDocumento": 12345678,
      "tipoDocumentoCodigo": "DNI",
      "genero": "Femenino"
    }
    ```
  - Ejemplo respuesta 201:
    ```json
    { "message": "Registro exitoso. Revisa tu correo para confirmar tu email.", "requiresEmailConfirmation": true }
    ```

- POST `/api/auth/register/patient`
  - Body JSON: igual que `register` + opcionales de paciente: `obraSocial?`, `numeroAfiliado?`, `contactoEmergenciaNombre?`, `contactoEmergenciaTelefono?`, `contactoEmergenciaRelacion?`
  - Efecto: crea PERSONAS, USUARIOS, PACIENTES y vincula rol PACIENTE en USUARIOS_ROLES.
  - Respuesta: `201 { message, requiresEmailConfirmation: true }`
  - Ejemplo request:
    ```json
    {
      "nombre": "Juan",
      "apellido": "Pérez",
      "email": "juan.perez@example.com",
      "password": "Password123",
      "telefono": "1234567890",
      "nroDocumento": 12345678,
      "tipoDocumentoCodigo": "DNI",
      "genero": "Masculino",
      "obraSocial": "OSDE",
      "numeroAfiliado": "123456789"
    }
    ```

- POST `/api/auth/register/professional`
  - Body JSON: igual que `register` + requeridos del profesional: `matriculaNacional`, `matriculaProvincial`, `especialidadId`, `titulo`, `universidad`, `cuit_cuil` (11 dígitos)
  - Validaciones: email único, matrículas únicas, CUIT/CUIL válido y único, `especialidadId` existente.
  - Efecto: crea PERSONAS, USUARIOS, PROFESIONALES, relación en PROFESIONAL_ESPECIALIDADES y vincula rol PROFESIONAL en USUARIOS_ROLES.
  - Respuesta: `201 { message, requiresEmailConfirmation: true }`
  - Ejemplo request:
    ```json
    {
      "nombre": "María",
      "apellido": "González",
      "email": "maria.gonzalez@example.com",
      "password": "Password123",
      "telefono": "1234567890",
      "nroDocumento": 87654321,
      "tipoDocumentoCodigo": "DNI",
      "genero": "Femenino",
      "matriculaNacional": "MN12345",
      "matriculaProvincial": "MP67890",
      "especialidadId": 1,
      "titulo": "Médico Cirujano",
      "universidad": "Universidad de Buenos Aires",
      "cuit_cuil": "20876543210"
    }
    ```

- GET `/api/auth/confirm-email?uid&token`
  - Marca el email como confirmado y redirige (o responde JSON si no hay redirección configurada).
  - Parámetros requeridos: `uid` (int), `token` (string Base64Url)
  - Respuesta JSON (si no hay redirección):
    - 200 OK: `{ "message": "Email confirmado" }`
    - 400/404/401: `{ "message": "..." }` (token inválido/expirado/usuario no encontrado)

- POST `/api/auth/resend-confirmation`
  - Body JSON: `{ email }`
  - Reenvía el correo de confirmación (respuesta genérica, respeta cooldown configurado).
  - Campos requeridos: `email` (formato email)
  - Ejemplo request:
    ```json
    { "email": "ana@example.com" }
    ```
  - Ejemplo respuesta 200 (genérica):
    ```json
    { "message": "Si el email está registrado y no confirmado, enviaremos un enlace de confirmación." }
    ```

- POST `/api/auth/login`
  - Body JSON: `{ email, password }`
  - Respuesta: `{ token, refreshToken, user, expiresIn }` (requiere email confirmado).
  - Campos requeridos: `email`, `password`
  - Ejemplo request:
    ```json
    { "email": "ana@example.com", "password": "Secreta1!" }
    ```
  - Ejemplo respuesta 200:
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiresIn": 3600,
      "user": {
        "id": "12",
        "email": "ana@example.com",
        "name": "Ana Pérez",
        "role": "patient",
        "status": "active",
        "phone": null,
        "avatar": null,
        "createdAt": "2025-10-24T13:20:00Z",
        "updatedAt": "2025-10-24T13:20:00Z"
      }
    }
    ```
  - Errores típicos:
    - 401 `{ "message": "Credenciales inválidas." }`
    - 401 `{ "message": "El email no está confirmado." }`

- GET `/api/auth/specialties`
  - Devuelve lista de especialidades médicas (id + nombre) para registro de profesionales.
  - Ejemplo respuesta:
    ```json
    [
      { "id": 1, "nombre": "Clínica Médica" },
      { "id": 2, "nombre": "Cardiología" }
    ]
    ```

- POST `/api/auth/refresh`
  - Body JSON: `{ refreshToken }`
  - Respuesta: `{ token, refreshToken, user, expiresIn }`.
  - Campos requeridos: `refreshToken`
  - Ejemplo request:
    ```json
    { "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
    ```
  - Ejemplo respuesta 200: igual que login (con nuevos tokens)
  - Error 401: `{ "message": "Refresh token inválido o expirado." }`

- GET `/api/auth/me`
  - Header: `Authorization: Bearer {token}`
  - Respuesta: `user`.
  - Ejemplo respuesta 200:
    ```json
    {
      "id": "12",
      "email": "ana@example.com",
      "name": "Ana Pérez",
      "role": "patient",
      "status": "active",
      "phone": null,
      "avatar": null,
      "createdAt": "2025-10-24T13:20:00Z",
      "updatedAt": "2025-10-24T13:20:00Z"
    }
    ```

Utilidad
- GET `/health` → estado de salud de la API/DB.
- GET `/` → información básica de la API.

## Configuración relevante (Development)
- Archivo: `BackendDoctorWare/DoctorWare/appsettings.Development.json`
- JWT: `Jwt:{ Secret, Issuer, Audience, AccessTokenMinutes, RefreshTokenMinutes }`
- Email SMTP: `Email:Smtp:{ Host, Port, EnableSsl, User, Password, FromEmail, FromName }`
- Confirmación de email: `EmailConfirmation:{ TokenMinutes (0 = sin vencimiento), FrontendConfirmUrl, BackendConfirmRedirectUrl, ResendCooldownSeconds }`

Logs: `BackendDoctorWare/DoctorWare/logs`.

Notas sobre roles
- Registro de Paciente: asigna rol `PACIENTE` en `USUARIOS_ROLES`.
- Registro de Profesional: asigna rol `PROFESIONAL` y vincula `especialidadId`.
- Claim `role` del JWT retorna nombres normalizados para frontend: `patient`, `professional`, `secretary`, `admin`.

## Guía Frontend: Uso de Endpoints de Auth

- Base URL en dev: `http://localhost:3000`
- Headers comunes:
  - `Content-Type: application/json`
  - `Accept: application/json`

Flujo recomendado
1) Registro público (paciente o profesional)
   - Paciente: `POST /api/auth/register/patient`
   - Profesional: `POST /api/auth/register/professional`
   - Respuesta 201: `{ message, requiresEmailConfirmation: true }`
2) Confirmación de email
   - Link llega por correo. Endpoint backend: `GET /api/auth/confirm-email?uid={id}&token={token}`
   - Si hay `EmailConfirmation:FrontendConfirmUrl` configurado, la app redirige a ese URL con `uid` y `token` como query.
3) Login
   - `POST /api/auth/login` con `{ email, password }`
   - Respuesta 200: `{ token, refreshToken, user, expiresIn }`
   - Guardar `token` (Bearer) y `refreshToken` en storage seguro.
4) Perfil (Me)
   - `GET /api/auth/me` con `Authorization: Bearer {token}`
   - Respuesta 200: `UserFrontendDto` (`id`, `email`, `name`, `role`, `status`, `phone`, `avatar`, `createdAt`, `updatedAt`)
5) Refresh token
   - `POST /api/auth/refresh` con `{ refreshToken }`
   - Respuesta 200: nuevos `{ token, refreshToken, user, expiresIn }`

Registro de Paciente (campos)
- Requeridos (heredados de `register`):
  - `nombre`: string, mínimo 2
  - `apellido`: string, mínimo 2
  - `email`: string (email válido)
  - `password`: string (mínimo 6)
  - `nroDocumento`: number
  - `tipoDocumentoCodigo`: string (ej. "DNI")
  - `genero`: string (ej. "Femenino", "Masculino", "Prefiere no decirlo")
- Opcionales específicos:
  - `obraSocial`: string
  - `numeroAfiliado`: string
  - `contactoEmergenciaNombre`: string
  - `contactoEmergenciaTelefono`: string
  - `contactoEmergenciaRelacion`: string

Registro de Profesional (campos)
- Requeridos (heredados de `register`) + específicos:
  - `matriculaNacional`: string (min 5)
  - `matriculaProvincial`: string (min 5)
  - `especialidadId`: number (ID en tabla ESPECIALIDADES)
  - `titulo`: string
  - `universidad`: string
  - `cuit_cuil`: string de 11 dígitos (valida DV, sin guiones)
- Validaciones backend:
  - Email único
  - Matrícula nacional única
  - Matrícula provincial única
  - CUIT/CUIL válido y único
  - `especialidadId` existente

Especialidades
- `GET /api/auth/specialties` → lista de `{ id, nombre }` ordenada por nombre.
- Usar para poblar select en formulario de registro de profesional.

Manejo de errores
- Formato estándar (middleware):
  ```json
  {
    "success": false,
    "data": null,
    "message": "El email ya está registrado.",
    "errorCode": "bad_request"
  }
  ```
- Códigos y mensajes comunes:
  - 400 `bad_request`: "El email ya está registrado.", "La matrícula nacional ya está registrada.", "La matrícula provincial ya está registrada.", "El CUIT/CUIL ya está registrado.", "CUIT/CUIL inválido.", "La especialidad indicada no existe."
  - 401 `unauthorized`: "Credenciales inválidas.", "El email no está confirmado.", "Refresh token inválido o expirado."
  - 404 `not_found`: recurso inexistente
  - 500 `server_error` o `db_connection_error`

Tokens y roles
- Header: `Authorization: Bearer {token}` en endpoints protegidos.
- `expiresIn`: segundos de vida del access token.
- El claim `role` estará normalizado para el front: `patient`, `professional`, `secretary`, `admin`.
- Recomendación: interceptor HTTP que refresque tokens al recibir 401 por expiración (ver ejemplo base en este README).

Notas de tiempo y formato
- Timestamps en UTC (`createdAt`, `updatedAt`).
- Strings JSON en `camelCase` por configuración del serializer.

## Manejo de errores en Angular (ejemplos)

- Interceptor para refrescar o manejar 401/403:

```ts
import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
        const message = (err.error && err.error.message) || err.message;
        if (err.status === 401 && message === 'El email no está confirmado.') {
          // Mostrar UI para reenviar confirmación
          // this.auth.resend(email).subscribe();
        }
        return throwError(() => err);
      })
    );
  }
}
```

- Ejemplo de flujo de login en componente:

```ts
this.auth.login(email, password).subscribe({
  next: (res) => {
    localStorage.setItem('doctorware_token', res.token);
    // navegar a dashboard
  },
  error: (err) => {
    const msg = err?.error?.message || 'Error de autenticación';
    if (msg === 'El email no está confirmado.') {
      // ofrecer botón: reenviar confirmación
      // this.auth.resend(email).subscribe();
    }
  }
});
```
