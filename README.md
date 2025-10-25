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
