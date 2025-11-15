# DoctorWare API Contract

Guía de consumo para el frontend (Angular). Esta especificación describe rutas, formatos y ejemplos de uso.

## Base y convenciones

- Base URL (dev): `http://localhost:3000/api`
- Headers comunes:
  - `Content-Type: application/json`
  - `Accept: application/json`
  - `Authorization: Bearer {token}` (en endpoints protegidos)
- Fechas/horas: ISO‑8601 (UTC). Ej.: `2025-11-15T10:00:00.000Z`
- Horas de turno: `HH:mm` (ej.: `09:30`)
- IDs en JSON: se exponen como `string` (compatibilidad con front)
- CORS (dev): `http://localhost:4200` (configurable por `Cors:AllowedOrigins`)

## Estructura de errores

Las respuestas de error tienen formato homogéneo:

```json
{
  "success": false,
  "data": { ...? },
  "message": "Texto descriptivo",
  "errorCode": "unauthorized|forbidden|not_found|method_not_allowed|bad_request|server_error|..."
}
```

Estados comunes:
- 400 `bad_request`
- 401 `unauthorized`
- 403 `forbidden`
- 404 `not_found`
- 405 `method_not_allowed`
- 500 `server_error`

---

## Autenticación

- POST `/auth/register`
  - Request: ver campos en README del backend (sección “APIs disponibles → Autenticación”)
  - 201: `{ "message": "...", "requiresEmailConfirmation": true }`

- POST `/auth/register/patient`
- POST `/auth/register/professional`

- GET `/auth/confirm-email?uid={id}&token={token}`

- POST `/auth/resend-confirmation`
  - Body: `{ "email": "user@example.com" }`

- POST `/auth/login`
  - Body: `{ "email": "user@test.com", "password": "123456" }`
  - 200: `{ token, refreshToken, user, expiresIn }`

- POST `/auth/refresh`
  - Body: `{ refreshToken }`
  - 200: `{ token, refreshToken, user, expiresIn }`

- GET `/auth/me`
  - 200: `UserFrontendDto`

Notas
- Tras login, enviar `Authorization: Bearer {token}` en endpoints protegidos.
- Guardar `refreshToken` para renovar sesiones.

---

## Especialidades

- GET `/specialties`
  - 200: `[{ id: number, nombre: string }]`

- GET `/specialties/{id}/subspecialties`
  - 200: `[{ id: number, nombre: string, specialtyId: number }]`

---

## Profesionales

- GET `/professionals?specialtyId={int}&name={string}`
  - 200: `[{ id, userId, name, matriculaNacional?, matriculaProvincial?, especialidad?, activo }]`
  - `userId` se utiliza como `professionalId` en el frontend (ID del usuario).

- GET `/professionals/{id}`

---

## Pacientes

- GET `/patients?name=&dni=&email=&phone=&professionalId=&isActive=`
  - `professionalId`: ID de usuario del profesional (el backend lo mapea a `PROFESIONALES`).
  - 200: `Patient[]` compatible con el modelo del front.

- GET `/patients/{id}`

- POST `/patients`
  - Body (CreatePatientDto del front):
    ```json
    {
      "name": "Juan Pérez",
      "email": "juan@...",
      "phone": "11...",
      "dni": "30123456",
      "dateOfBirth": "1990-05-02T00:00:00.000Z",
      "gender": "male",
      "address": {"street":"Calle 123","city":"CABA","state":"BA","zipCode":"1000","country":"AR"},
      "emergencyContact": {"name":"Ana","phone":"...","relationship":"Esposa"},
      "medicalInsurance": {"provider":"OSDE","planName":"310","memberNumber":"ABC"},
      "professionalId": "45"
    }
    ```

- PUT `/patients/{id}` (UpdatePatientDto)

- DELETE `/patients/{id}`

- GET `/patients/summary`
  - 200: `[{ id, name, age, dni, phone, lastAppointment?, nextAppointment?, totalAppointments, activeConditions }]`

- GET `/patients/{id}/history`
  - Alias conveniente a historia clínica del paciente.

---

## Turnos (Appointments)

- GET `/appointments?professionalId=&patientId=&startDate=&endDate=&status=&type=`
  - `status`: `scheduled|confirmed|in_progress|completed|cancelled|no_show`
  - `type`: `first_visit|follow_up|emergency|routine|specialist`

- GET `/appointments/{id}`

- POST `/appointments`
  - Body:
    ```json
    {
      "patientId": "100",
      "professionalId": "45",
      "date": "2025-11-15T00:00:00.000Z",
      "startTime": "09:00",
      "duration": 30,
      "type": "first_visit",
      "reason": "Control anual",
      "notes": "Traer estudios"
    }
    ```

- PUT `/appointments/{id}`
  - Body parcial (UpdateAppointmentDto)

- DELETE `/appointments/{id}` [`?reason=`]

- GET `/appointments/stats?professionalId=`
  - 200: `{ total, scheduled, confirmed, completed, cancelled, noShow }`

Mapeos DB ↔ Front (interno)
- Estados: `Programado↔scheduled`, `Confirmado↔confirmed`, `En Espera↔in_progress`, `Atendido↔completed`, `Cancelado↔cancelled`, `Ausente↔no_show`
- Tipos: `Consulta↔first_visit`, `Seguimiento↔follow_up`, `Estudio↔specialist`, `Cirugía/Control General/Vacunación↔routine`

---

## Historia Clínica

- GET `/medical-history/patient/{patientId}` → `MedicalHistory[]`
- GET `/medical-history/{id}` → `MedicalHistory`
- POST `/medical-history`
  - Body (CreateMedicalHistoryDto):
    ```json
    {
      "patientId": "100",
      "appointmentId": "123",
      "type": "consultation",
      "date": "2025-11-15T13:00:00Z",
      "title": "Consulta general",
      "description": "Dolor torácico",
      "diagnosis": "Gastroenteritis",
      "treatment": "Reposo",
      "observations": "Reevaluar 48h",
      "attachments": []
    }
    ```
- PUT `/medical-history/{id}` (UpdateMedicalHistoryDto)
- DELETE `/medical-history/{id}`

Notas
- `type` y `attachments` se persisten en un campo JSON interno y se devuelven en la respuesta.

---

## Diagnósticos

- GET `/diagnoses/patient/{patientId}` → `Diagnosis[]`
- POST `/diagnoses` (CreateDiagnosisDto)
- PUT `/diagnoses/{id}` (UpdateDiagnosisDto)

```jsonc
// CreateDiagnosisDto
{
  "patientId": "100",
  "appointmentId": "123",
  "code": "J10",
  "name": "Gripe",
  "description": "Cuadro febril",
  "severity": "moderate",
  "diagnosisDate": "2025-11-15T13:00:00Z",
  "status": "active",
  "notes": "Indicar reposo"
}
```

---

## Alergias

- GET `/allergies/patient/{patientId}` → `Allergy[]`
- POST `/allergies` (CreateAllergyDto)
- PUT `/allergies/{id}` (UpdateAllergyDto)
- PATCH `/allergies/{id}/deactivate`

```jsonc
// CreateAllergyDto
{
  "patientId": "100",
  "allergen": "Penicilina",
  "type": "medication",
  "severity": "high",
  "symptoms": "Urticaria",
  "diagnosedDate": "2020-01-01T00:00:00Z",
  "notes": "Evitar betalactámicos"
}
```

---

## Medicación

- GET `/medications/patient/{patientId}` → `Medication[]`
- POST `/medications` (CreateMedicationDto)
- PUT `/medications/{id}` (UpdateMedicationDto)
- PATCH `/medications/{id}/discontinue`

```jsonc
// CreateMedicationDto
{
  "patientId": "100",
  "appointmentId": "123",
  "medicationName": "Ibuprofeno",
  "dosage": "400 mg",
  "frequency": "Cada 8 hs",
  "duration": "5 días",
  "startDate": "2025-11-15T00:00:00Z",
  "endDate": "2025-11-20T00:00:00Z",
  "instructions": "Tomar con comida"
}
```

---

## Ejemplos Angular (HttpClient)

```ts
// Login
this.http.post<AuthResponse>(`${environment.apiBaseUrl}/Auth/login`, { email, password })

// Listar turnos
this.http.get<Appointment[]>(`${environment.apiBaseUrl}/appointments`, {
  params: { professionalId, startDate: from.toISOString(), endDate: to.toISOString(), status: 'confirmed' }
});

// Crear paciente
this.http.post<Patient>(`${environment.apiBaseUrl}/patients`, dto)
```

---

## Notas de integración

- `professionalId` en query/body siempre es el ID de usuario (claim `sub` en JWT). El backend lo resuelve al `ID_PROFESIONALES`.
- Serialización JSON en camelCase, timezone UTC.
- Swagger: `http://localhost:3000/swagger`

