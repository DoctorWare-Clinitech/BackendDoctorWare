# DoctorWare API – Guía rápida de endpoints

Resumen práctico para consumir cada ruta desde Angular u otro cliente HTTP. Los ejemplos asumen `Base URL = http://localhost:3000/api`.

## 1. Autenticación

| Método | Ruta | Uso |
| --- | --- | --- |
| `POST` | `/auth/register` | Registro genérico. Body: `{ nombre, apellido, email, password, telefono?, nroDocumento, tipoDocumentoCodigo, genero }`. Devuelve `201 { message, requiresEmailConfirmation }`. |
| `POST` | `/auth/register/patient` | Registro completo de paciente (incluye obra social/contacto). Respuesta igual al registro genérico. |
| `POST` | `/auth/register/professional` | Registro de profesional con matrículas, especialidad, título, CUIT/CUIL. Respuesta `201`. |
| `GET` | `/auth/confirm-email?uid=&token=` | Marca el email como confirmado; responde `{ message }` o redirige según configuración. |
| `POST` | `/auth/resend-confirmation` | Body: `{ email }`. Devuelve mensaje genérico. |
| `POST` | `/auth/login` | Body: `{ email, password }`. Respuesta: `{ token, refreshToken, user, expiresIn }`. |
| `POST` | `/auth/refresh` | Body: `{ refreshToken }`. Devuelve nuevo par de tokens. |
| `GET` | `/auth/me` | Requiere `Authorization: Bearer`. Respuesta `UserFrontendDto`. |
| `POST` | `/auth/forgot-password` | Body: `{ email }`. Siempre responde `200 { message }`. Si el email existe envía link por mail. |
| `POST` | `/auth/reset-password` | Body: `{ token, newPassword }`. Respuesta `200 { message }`. Valida token de recuperación. |
| `GET` | `/auth/specialties` | Lista especialidades médicas (`[{ id, nombre }]`). Útil para formularios de registro. |

## 2. Profesionales y especialidades

- `GET /specialties` y `GET /specialties/{id}/subspecialties` → catálogos.
- `GET /professionals?specialtyId=&name=` → busca profesionales. Respuesta incluye `userId` (usar como `professionalId` en filtros del backend).  
- `GET /professionals/{id}` → detalle público.

## 3. Portal público (sin autenticación)

| Método | Ruta | Uso |
| --- | --- | --- |
| `GET` | `/public/professionals/{professionalId}/availability?date=YYYY-MM-DD` | Devuelve lista de `ScheduleAvailableSlotDto` con `date`, `time`, `duration`, `available`, `appointmentId`. Úsalo para pintar agenda pública. |
| `POST` | `/public/appointments` | Crea turno sin login. Body: `{ professionalId, date, startTime, duration?, type?, reason?, notes?, patient:{ firstName,lastName,email,phone,dni,dateOfBirth,gender } }`. Opcionales: `existingPatientId`, `existingPatientUserId`. Respuesta `201 AppointmentDto`. |

## 4. Portal paciente autenticado (JWT rol `patient`)

- `GET /me/appointments?startDate=&endDate=` → lista de turnos propios ordenados.
- `GET /me/appointments/{id}` → detalle (404 si el turno no pertenece al paciente).
- `DELETE /me/appointments/{id}?reason=` → cancela su propio turno.
- `GET /me/history` → entradas de historia clínica asociadas al paciente.

## 5. Pacientes (roles `professional|secretary|admin`)

| Método | Ruta | Comentario |
| --- | --- | --- |
| `GET /patients?name=&dni=&email=&phone=&professionalId=&isActive=` | Filtros opcionales. Requiere `Authorization`. |
| `GET /patients/{id}` | Detalle. |
| `POST /patients` | Body `CreatePatientRequest`. Crea persona + paciente y opcionalmente asigna profesional (pasar `professionalId` = userId del médico). |
| `PUT /patients/{id}` | Actualiza datos básicos. |
| `DELETE /patients/{id}` | Elimina. |
| `GET /patients/summary?professionalId=` | Resumen por profesional. |
| `GET /patients/{id}/history` | Alias de historia clínica del paciente. |

## 6. Turnos (roles `professional|secretary|admin`)

- `GET /appointments?professionalId=&patientId=&startDate=&endDate=&status=&type=` → listados con filtros (usar `professionalId` = userId).
- `GET /appointments/{id}` → detalle.
- `POST /appointments` → Body `CreateAppointmentRequest`: `{ patientId, professionalId (userId), date, startTime, duration, type?, reason?, notes? }`.
- `PUT /appointments/{id}` → Body parcial `UpdateAppointmentRequest`.
- `DELETE /appointments/{id}?reason=` → cancela.
- `GET /appointments/stats?professionalId=` → totales.

## 7. Historia clínica y registros médicos

| Módulo | Rutas principales |
| --- | --- |
| Historia clínica | `GET /medical-history/patient/{patientId}`, `GET /medical-history/{id}`, `POST`, `PUT`, `DELETE`. Body `CreateMedicalHistoryDto`. |
| Diagnósticos | `GET /diagnoses/patient/{patientId}`, `POST`, `PUT`. Cada entrada es un `MedicalHistory` con metadatos. |
| Alergias | `GET /allergies/patient/{patientId}`, `POST`, `PUT`, `PATCH /{id}/deactivate`. |
| Medicación | `GET /medications/patient/{patientId}`, `POST`, `PUT`, `PATCH /{id}/discontinue`. |

## 8. Agenda y ausencias (roles `professional|secretary|admin`)

- `GET /schedule/{professionalUserId}` → configuración de agenda (slots + duración).
- `PUT /schedule/{professionalUserId}` → reemplaza configuración (slots y duración).
- `POST /schedule/{professionalUserId}/slots` / `PUT /slots/{slotId}` / `DELETE /slots/{slotId}` → CRUD de horarios laborables.
- `POST /schedule/{professionalId}/blocks` / `DELETE /blocks/{blockId}` / `GET /blocks?startDate=&endDate=` → bloqueos (ausencias).
- `GET /schedule/{professionalId}/available?date=` → slots disponibles (uso interno; el portal público consume la versión `/api/public/...`).

## 9. Notificaciones de turnos

- El worker `AppointmentReminderWorker` ejecuta el servicio `AppointmentReminderService` según `Appointments:Reminder` en `appsettings`. No expone endpoint, pero conviene monitorear logs (`logs/`).

## 10. Métricas y salud

- `GET /metrics/summary` (roles `admin|professional`): retorna `{ totalRequests, averageMilliseconds, maxMilliseconds, requestsByPath, generatedAtUtc }`.
- `GET /health` → estado de API/DB.
- `GET /` → información básica de la API.
- Swagger: `http://localhost:3000/swagger`.

## 11. Consideraciones generales

- **Cabeceras comunes**: `Content-Type: application/json`, `Accept: application/json`. Endpoints protegidos requieren `Authorization: Bearer {token}`.
- **Fechas**: enviar ISO-8601 UTC, p.ej. `2025-11-15T10:00:00Z`. Horas de turnos como `HH:mm`.
- **Roles**: `professional`, `secretary`, `patient`, `admin`. El claim se resuelve en `ResolvePrimaryRoleAsync`.
- **Configuración sensible**: `DataProtection:Key` (cifrado de notas/contactos), `PasswordReset:{FrontEndResetUrl,TokenMinutes,TokenSecret}`, `Jwt:{Secret,Issuer,Audience,...}` y `Email:Smtp` para notificaciones.

Con esta guía podés mapear cada pantalla de Angular a su endpoint correspondiente y saber qué payload enviar/recibir.
