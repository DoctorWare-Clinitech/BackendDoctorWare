# Guía para Frontend — Consumo de la API (DoctorWare)

Esta guía reúne TODOS los endpoints disponibles, sus parámetros, cuerpos esperados y ejemplos de uso (cURL y Angular HttpClient).

- Base URL (dev): `http://localhost:3000/api`
- Headers comunes:
  - `Content-Type: application/json`
  - `Accept: application/json`
  - `Authorization: Bearer {token}` (endpoints protegidos)
- Convenciones:
  - Fechas/horas en ISO‑8601 (UTC). Ej.: `2025-11-15T10:00:00Z`
  - Horarios `HH:mm`
  - IDs expuestos como `string`
- Errores (formato homogéneo):
  ```json
  { "success": false, "data": null, "message": "Texto", "errorCode": "bad_request|unauthorized|forbidden|not_found|..." }
  ```

## 1) Autenticación

POST `/auth/login`
- Body `{ email, password }`
- 200 `{ token, refreshToken, user, expiresIn }`
```bash
curl -X POST $API/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"user@test.com","password":"123456"}'
```

GET `/auth/me` (Bearer)
```bash
curl $API/auth/me -H "Authorization: Bearer $TOKEN"
```

POST `/auth/refresh`
- Body `{ refreshToken }`

POST `/auth/resend-confirmation`
- Body `{ email }`

GET `/auth/confirm-email?uid={id}&token={token}`

POST `/auth/register`
- Ver README y API_CONTRACT para campos.

Sugerencia Angular (JWT Interceptor)
```ts
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const t = localStorage.getItem('doctorware_token');
    return next.handle(t ? req.clone({ setHeaders: { Authorization: `Bearer ${t}` } }) : req);
  }
}
```

## 2) Especialidades y Profesionales

GET `/specialties`
GET `/specialties/{id}/subspecialties`

GET `/professionals?specialtyId={int}&name={string}`
GET `/professionals/{id}`

Angular (fragmento)
```ts
@Injectable({ providedIn: 'root' })
export class ProfessionalsApi {
  private base = `${env.api}/professionals`;
  constructor(private http: HttpClient) {}
  list(params: { specialtyId?: number; name?: string }) { return this.http.get<any[]>(this.base, { params }); }
  get(id: number) { return this.http.get<any>(`${this.base}/${id}`); }
}
```

## 3) Pacientes

GET `/patients?name=&dni=&email=&phone=&professionalId=&isActive=`
```bash
curl "$API/patients?name=juan&professionalId=45"
```

GET `/patients/{id}`

POST `/patients`
```json
{
  "name": "Juan Pérez",
  "email": "juan@...",
  "phone": "11...",
  "dni": "30123456",
  "dateOfBirth": "1990-05-02T00:00:00Z",
  "gender": "male",
  "address": {"street":"Calle 123","city":"CABA","state":"BA","zipCode":"1000","country":"AR"},
  "emergencyContact": {"name":"Ana","phone":"...","relationship":"Esposa"},
  "medicalInsurance": {"provider":"OSDE","planName":"310","memberNumber":"ABC"},
  "professionalId": "45"
}
```

PUT `/patients/{id}` (parcial)
DELETE `/patients/{id}`
GET `/patients/summary`
GET `/patients/{id}/history`

Angular (fragmento)
```ts
@Injectable({ providedIn: 'root' })
export class PatientsApi {
  private base = `${env.api}/patients`;
  constructor(private http: HttpClient) {}
  list(params: any) { return this.http.get<PatientDto[]>(this.base, { params }); }
  get(id: string) { return this.http.get<PatientDto>(`${this.base}/${id}`); }
  create(p: CreatePatientDto) { return this.http.post<PatientDto>(this.base, p); }
  update(id: string, p: UpdatePatientDto) { return this.http.put<PatientDto>(`${this.base}/${id}`, p); }
  remove(id: string) { return this.http.delete(`${this.base}/${id}`); }
  summary() { return this.http.get<any[]>(`${this.base}/summary`); }
  history(id: string) { return this.http.get<any[]>(`${this.base}/${id}/history`); }
}
```

## 4) Turnos (Appointments)

GET `/appointments?professionalId=&patientId=&startDate=&endDate=&status=&type=`
GET `/appointments/{id}`
POST `/appointments`
```json
{
  "patientId": "100",
  "professionalId": "45",
  "date": "2025-11-15T00:00:00Z",
  "startTime": "09:00",
  "duration": 30,
  "type": "first_visit",
  "reason": "Control anual",
  "notes": "Traer estudios"
}
```
PUT `/appointments/{id}`
DELETE `/appointments/{id}` [`?reason=`]
GET `/appointments/stats?professionalId=` → `{ total, scheduled, confirmed, completed, cancelled, noShow }`

Angular (fragmento)
```ts
@Injectable({ providedIn: 'root' })
export class AppointmentsApi {
  private base = `${env.api}/appointments`;
  constructor(private http: HttpClient) {}
  list(params: any) { return this.http.get<AppointmentDto[]>(this.base, { params }); }
  get(id: string) { return this.http.get<AppointmentDto>(`${this.base}/${id}`); }
  create(p: CreateAppointmentDto) { return this.http.post<AppointmentDto>(this.base, p); }
  update(id: string, p: UpdateAppointmentDto) { return this.http.put<AppointmentDto>(`${this.base}/${id}`, p); }
  cancel(id: string, reason?: string) {
    const options = reason ? { params: new HttpParams().set('reason', reason) } : {};
    return this.http.delete(`${this.base}/${id}`, options);
  }
  stats(professionalId?: string) { return this.http.get<any>(`${this.base}/stats`, { params: { professionalId: professionalId ?? '' } }); }
}
```

## 5) Historia Clínica

GET `/medical-history/patient/{patientId}`
GET `/medical-history/{id}`
POST `/medical-history`
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
PUT `/medical-history/{id}`
DELETE `/medical-history/{id}`

## 6) Diagnósticos

GET `/diagnoses/patient/{patientId}`
POST `/diagnoses`
```json
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
PUT `/diagnoses/{id}`

## 7) Alergias

GET `/allergies/patient/{patientId}`
POST `/allergies`
```json
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
PUT `/allergies/{id}`
PATCH `/allergies/{id}/deactivate`

## 8) Medicación

GET `/medications/patient/{patientId}`
POST `/medications`
```json
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
PUT `/medications/{id}`
PATCH `/medications/{id}/discontinue`

---

Notas
- Todos los endpoints anteriores están documentados también en `BackendDoctorWare/API_CONTRACT.md`.
- Asegura CORS en Development con `Cors:AllowedOrigins`.
