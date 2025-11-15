# DoctorWare API (Backend)

Backend ASP.NET Core + Dapper + PostgreSQL.

Documentación de consumo para frontend: ver `BackendDoctorWare/API_CONTRACT.md`.

Guía completa de uso/desarrollo (incluye DRY con bases): `BackendDoctorWare/DoctorWare/docs/USAGE.md`.
Guía para frontend (consumo de API): `BackendDoctorWare/DoctorWare/docs/FRONTEND_API.md`.
Colecciones para clientes HTTP:
- Postman: `BackendDoctorWare/DoctorWare/docs/collections/DoctorWare.postman_collection.json`
- Insomnia: `BackendDoctorWare/DoctorWare/docs/collections/DoctorWare.insomnia.yaml`

Cómo importar
- Postman: Import → File → seleccionar el `.postman_collection.json`. Variables predefinidas: `api`, `token`, `patientId`, etc. Ejecuta "Auth → Login" para setear `token`.
- Insomnia: Import/Export → Import Data → From File → seleccionar el `.insomnia.yaml`.

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

- POST `/api/auth/forgot-password`
  - Body: `{ "email": "usuario@example.com" }`
  - Siempre responde 200 con mensaje genérico para evitar enumeración.

- POST `/api/auth/reset-password`
  - Body: `{ "token": "abc...", "newPassword": "NuevoPass123" }`
  - Valida token vigente y actualiza la contraseña.

Utilidad
- GET `/health` → estado de salud de la API/DB.
- GET `/` → información básica de la API.

Contrato de API detallado para frontend: `BackendDoctorWare/API_CONTRACT.md`.

Especialidades y Profesionales
- GET `/api/specialties` → lista de especialidades (id, nombre)
- GET `/api/specialties/{id}/subspecialties` → sub‑especialidades de una especialidad
- GET `/api/professionals?specialtyId=&name=` → lista de profesionales (incluye `userId` asociado)

Turnos (Appointments)
- GET `/api/appointments` → lista de turnos (filtros opcionales: `professionalId` [ID de usuario], `patientId`, `startDate`, `endDate`, `status`, `type`)
- GET `/api/appointments/{id}` → turno por id
- POST `/api/appointments` → crea un turno
- PUT `/api/appointments/{id}` → actualiza campos (fecha/hora/duración/estado/tipo/motivo/notas/observaciones)
- DELETE `/api/appointments/{id}` → cancela un turno
- GET `/api/appointments/stats` → estadísticas agregadas (acepta `professionalId`)

Notas
- El filtro `professionalId` espera el ID de usuario (JWT `sub`) y el backend lo traduce internamente al `ID_PROFESIONALES` correspondiente.
- Estados mapeados (DB → Front): Programado→`scheduled`, Confirmado→`confirmed`, En Espera→`in_progress`, Atendido→`completed`, Cancelado→`cancelled`, Ausente→`no_show`.
- Tipos mapeados (DB → Front): Consulta→`first_visit`, Seguimiento→`follow_up`, Estudio→`specialist`, Cirugía/Control General/Vacunación→`routine`.

Portal público (pacientes)
- GET `/api/public/professionals/{professionalId}/availability?date=2025-11-15`
  - Devuelve los `ScheduleAvailableSlotDto` del profesional (lista de horas con `available`, `appointmentId` y `duration`).
  - No requiere autenticación. Ideal para mostrar disponibilidad antes de registrarse/iniciar sesión.
- POST `/api/public/appointments`
  - Permite solicitar un turno desde el portal público. Body:
    ```json
    {
      "professionalId": "45",
      "date": "2025-11-15T00:00:00Z",
      "startTime": "09:00",
      "duration": 30,
      "type": "first_visit",
      "reason": "Control general",
      "patient": {
        "firstName": "Juan",
        "lastName": "Pérez",
        "email": "juan@example.com",
        "phone": "1160000000",
        "dni": "30123456",
        "dateOfBirth": "1990-05-02T00:00:00Z",
        "gender": "male"
      }
    }
    ```
  - Si el paciente ya existe (por `ExistingPatientId`, `ExistingPatientUserId`, DNI o email) se reutiliza; si no, se crea con los datos provistos.

Portal autenticado (pacientes)
- GET `/api/me/appointments[?startDate=&endDate=]` → lista de turnos del paciente autenticado.
- GET `/api/me/appointments/{id}` → detalle del turno (valida pertenencia).
- DELETE `/api/me/appointments/{id}` → cancela su turno (opcional `reason`).
- GET `/api/me/history` → entradas de historia clínica propias.

Métricas y observabilidad
- GET `/api/metrics/summary` (roles `admin` o `professional`) → brinda `totalRequests`, `averageMilliseconds`, `maxMilliseconds` y el desglose por endpoint.
- Middleware `RequestMetricsMiddleware` registra la latencia de cada request y persiste un snapshot en memoria.

Pacientes
- GET `/api/patients` → lista con filtros (`name`, `dni`, `email`, `phone`, `professionalId`, `isActive`)
- GET `/api/patients/{id}` → paciente por id
- POST `/api/patients` → crea paciente (crea `PERSONAS` y `PACIENTES`); usa `professionalId` (ID de usuario del profesional) para asignar médico de cabecera
- PUT `/api/patients/{id}` → actualiza datos básicos, contacto, seguro, notas, activo
- DELETE `/api/patients/{id}` → elimina paciente
- GET `/api/patients/summary` → resumen básico por profesional
- GET `/api/patients/{id}/history` → alias de historia clínica del paciente

Historia Clínica
- GET `/api/medical-history/patient/{patientId}` → entradas de historia clínica
- GET `/api/medical-history/{id}` → entrada por id
- POST `/api/medical-history` → crea entrada; almacena `type` y `attachments` en JSON de `ADJUNTOS`
- PUT `/api/medical-history/{id}` → actualiza campos y adjuntos
- DELETE `/api/medical-history/{id}` → elimina entrada

Diagnósticos / Alergias / Medicación
- Diagnósticos
  - GET `/api/diagnoses/patient/{patientId}`
  - POST `/api/diagnoses`
  - PUT `/api/diagnoses/{id}`
  - Nota: se persisten como entradas de `HISTORIAS_CLINICAS` con `ADJUNTOS` JSON (`type: 'diagnosis'`, `code`, `name`, `severity`, `diagnosisDate`, `status`, `notes`).
- Alergias
  - GET `/api/allergies/patient/{patientId}`
  - POST `/api/allergies`
  - PUT `/api/allergies/{id}`
  - PATCH `/api/allergies/{id}/deactivate`
  - Nota: se usa `PACIENTE_ALERGIAS` + `ALERGIAS`; metadatos en `DETALLES` JSON (`type`, `severity`, `symptoms`, `diagnosedDate`, `notes`, `active`).
- Medicación
  - GET `/api/medications/patient/{patientId}`
  - POST `/api/medications`
  - PUT `/api/medications/{id}`
  - PATCH `/api/medications/{id}/discontinue`
  - Nota: se persisten como entradas de `HISTORIAS_CLINICAS` con `ADJUNTOS` JSON (`type: 'medication'`, `medicationName`, `dosage`, `frequency`, `duration`, `startDate`, `endDate`, `instructions`, `active`).

## Configuración relevante (Development)
- Archivo: `BackendDoctorWare/DoctorWare/appsettings.Development.json`
- JWT: `Jwt:{ Secret, Issuer, Audience, AccessTokenMinutes, RefreshTokenMinutes }`
- Email SMTP: `Email:Smtp:{ Host, Port, EnableSsl, User, Password, FromEmail, FromName }`
- Confirmación de email: `EmailConfirmation:{ TokenMinutes (0 = sin vencimiento), FrontendConfirmUrl, BackendConfirmRedirectUrl, ResendCooldownSeconds }`
- Cifrado en reposo: `DataProtection:Key` (32 caracteres hex recomendados) para encriptar notas y contactos sensibles.
- Recuperación de contraseña: `PasswordReset:{ FrontendResetUrl, TokenMinutes (60 por defecto), TokenSecret }`

Logs: `BackendDoctorWare/DoctorWare/logs`. Métricas básicas en `/api/metrics/summary`.

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
