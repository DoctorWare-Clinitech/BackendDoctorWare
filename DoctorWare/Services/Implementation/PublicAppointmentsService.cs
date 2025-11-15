using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests.Appointments;
using DoctorWare.DTOs.Requests.Patients;
using DoctorWare.DTOs.Requests.Public;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.DTOs.Response.Patients;
using DoctorWare.Exceptions;
using DoctorWare.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DoctorWare.Services.Implementation
{
    public class PublicAppointmentsService : IPublicAppointmentsService
    {
        private readonly IAppointmentsService appointmentsService;
        private readonly IPatientsService patientsService;
        private readonly IDbConnectionFactory factory;
        private readonly IPatientIdentityService patientIdentityService;
        private readonly ILogger<PublicAppointmentsService> logger;

        public PublicAppointmentsService(
            IAppointmentsService appointmentsService,
            IPatientsService patientsService,
            IDbConnectionFactory factory,
            IPatientIdentityService patientIdentityService,
            ILogger<PublicAppointmentsService> logger)
        {
            this.appointmentsService = appointmentsService;
            this.patientsService = patientsService;
            this.factory = factory;
            this.patientIdentityService = patientIdentityService;
            this.logger = logger;
        }

        public async Task<AppointmentDto> RequestAppointmentAsync(PublicAppointmentRequest request, CancellationToken ct)
        {
            ValidateRequest(request);

            int patientId = await ResolvePatientIdAsync(request, ct);
            int duration = await ResolveDurationAsync(request, ct);

            CreateAppointmentRequest createRequest = new CreateAppointmentRequest
            {
                PatientId = patientId.ToString(CultureInfo.InvariantCulture),
                ProfessionalId = request.ProfessionalId,
                Date = request.Date.Date,
                StartTime = request.StartTime,
                Duration = duration,
                Type = string.IsNullOrWhiteSpace(request.Type) ? "first_visit" : request.Type,
                Reason = request.Reason,
                Notes = request.Notes
            };

            AppointmentDto appointment = await appointmentsService.CreateAsync(createRequest, createdByUserId: null, ct);
            logger.LogInformation(
                "Portal público: se solicitó turno para profesional {Profesional} y paciente {Paciente} el {Fecha} {Hora}",
                request.ProfessionalId,
                patientId,
                request.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                request.StartTime);

            return appointment;
        }

        private static void ValidateRequest(PublicAppointmentRequest request)
        {
            if (request is null)
            {
                throw new BadRequestException("Solicitud inválida.");
            }

            if (string.IsNullOrWhiteSpace(request.ProfessionalId))
            {
                throw new BadRequestException("Debe indicar un profesional.");
            }

            if (request.Patient is null)
            {
                throw new BadRequestException("Debe proporcionar los datos del paciente.");
            }

            if (string.IsNullOrWhiteSpace(request.Patient.FirstName) || string.IsNullOrWhiteSpace(request.Patient.LastName))
            {
                throw new BadRequestException("Nombre y apellido del paciente son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(request.Patient.Dni))
            {
                throw new BadRequestException("El DNI del paciente es obligatorio.");
            }

            if (!request.Patient.DateOfBirth.HasValue)
            {
                throw new BadRequestException("Debe indicar la fecha de nacimiento del paciente.");
            }

            if (string.IsNullOrWhiteSpace(request.StartTime))
            {
                throw new BadRequestException("Debe indicar la hora del turno.");
            }

            if (!TimeSpan.TryParse(request.StartTime, CultureInfo.InvariantCulture, out _))
            {
                throw new BadRequestException("Formato de hora inválido (HH:mm).");
            }
        }

        private async Task<int> ResolvePatientIdAsync(PublicAppointmentRequest request, CancellationToken ct)
        {
            if (int.TryParse(request.ExistingPatientId, out int explicitId))
            {
                return explicitId;
            }

            if (int.TryParse(request.ExistingPatientUserId, out int userId))
            {
                int? byUser = await patientIdentityService.GetPatientIdByUserIdAsync(userId, ct);
                if (byUser.HasValue)
                {
                    return byUser.Value;
                }
            }

            int? byDocument = await FindPatientByDocumentOrEmailAsync(request.Patient, ct);
            if (byDocument.HasValue)
            {
                return byDocument.Value;
            }

            PatientDto created = await CreatePatientAsync(request, ct);
            if (!int.TryParse(created.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out int createdId))
            {
                throw new InvalidOperationException("No se pudo obtener el identificador del paciente creado.");
            }

            return createdId;
        }

        private async Task<int?> FindPatientByDocumentOrEmailAsync(PublicPatientInfo info, CancellationToken ct)
        {
            using System.Data.IDbConnection con = factory.CreateConnection();

            if (!string.IsNullOrWhiteSpace(info.Dni) && long.TryParse(info.Dni, out long dni))
            {
                const string sqlDoc = @"
                    select pa.""ID_PACIENTES""
                    from public.""PACIENTES"" pa
                    join public.""PERSONAS"" per on per.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                    where per.""NRO_DOCUMENTO"" = @dni
                    limit 1;";

                int? fromDoc = await con.ExecuteScalarAsync<int?>(sqlDoc, new { dni });
                if (fromDoc.HasValue)
                {
                    return fromDoc.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(info.Email))
            {
                const string sqlEmail = @"
                    select pa.""ID_PACIENTES""
                    from public.""PACIENTES"" pa
                    join public.""PERSONAS"" per on per.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                    where lower(per.""EMAIL_PRINCIPAL"") = lower(@email)
                    limit 1;";

                int? fromEmail = await con.ExecuteScalarAsync<int?>(sqlEmail, new { email = info.Email });
                if (fromEmail.HasValue)
                {
                    return fromEmail.Value;
                }
            }

            return null;
        }

        private async Task<PatientDto> CreatePatientAsync(PublicAppointmentRequest request, CancellationToken ct)
        {
            PublicPatientInfo patient = request.Patient;
            string fullName = $"{patient.FirstName} {patient.LastName}".Trim();

            CreatePatientRequest create = new CreatePatientRequest
            {
                Name = fullName,
                Email = string.IsNullOrWhiteSpace(patient.Email) ? null : patient.Email,
                Phone = patient.Phone,
                Dni = patient.Dni,
                DateOfBirth = patient.DateOfBirth!.Value.Date,
                Gender = string.IsNullOrWhiteSpace(patient.Gender) ? "prefer_not_to_say" : patient.Gender,
                Address = new AddressRequest(),
                EmergencyContact = new EmergencyContactRequest
                {
                    Name = fullName,
                    Phone = patient.Phone,
                    Relationship = "Self",
                    Email = patient.Email
                },
                MedicalInsurance = null,
                ProfessionalId = request.ProfessionalId,
                Notes = patient.Notes
            };

            logger.LogInformation(
                "Portal público: creando paciente {Nombre} asociado al profesional {Profesional}",
                fullName,
                request.ProfessionalId);

            PatientDto created = await patientsService.CreateAsync(create, ct);
            return created;
        }

        private async Task<int> ResolveDurationAsync(PublicAppointmentRequest request, CancellationToken ct)
        {
            if (request.Duration.HasValue && request.Duration.Value > 0)
            {
                return request.Duration.Value;
            }

            using System.Data.IDbConnection con = factory.CreateConnection();
            if (!int.TryParse(request.ProfessionalId, out int professionalUserId))
            {
                return 30;
            }

            const string sql = @"
                select coalesce(p.""DURACION_TURNO_EN_MINUTOS"", 30)
                from public.""PROFESIONALES"" p
                join public.""USUARIOS"" u on u.""ID_PERSONAS"" = p.""ID_PERSONAS""
                where u.""ID_USUARIOS"" = @uid
                limit 1;";

            int? duration = await con.ExecuteScalarAsync<int?>(sql, new { uid = professionalUserId });
            return duration.GetValueOrDefault(30);
        }
    }
}
