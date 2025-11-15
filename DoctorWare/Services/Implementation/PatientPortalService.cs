using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.DTOs.Response.Medical;
using DoctorWare.Exceptions;
using DoctorWare.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DoctorWare.Services.Implementation
{
    public class PatientPortalService : IPatientPortalService
    {
        private readonly IPatientIdentityService patientIdentityService;
        private readonly IAppointmentsService appointmentsService;
        private readonly IMedicalHistoryService medicalHistoryService;
        private readonly ILogger<PatientPortalService> logger;

        public PatientPortalService(
            IPatientIdentityService patientIdentityService,
            IAppointmentsService appointmentsService,
            IMedicalHistoryService medicalHistoryService,
            ILogger<PatientPortalService> logger)
        {
            this.patientIdentityService = patientIdentityService;
            this.appointmentsService = appointmentsService;
            this.medicalHistoryService = medicalHistoryService;
            this.logger = logger;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(int userId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            string patientId = await ResolvePatientIdAsync(userId, ct);
            IEnumerable<AppointmentDto> list = await appointmentsService.GetAsync(null, patientId, startDate, endDate, null, null, ct);
            return list.OrderBy(a => a.Date).ThenBy(a => a.StartTime, StringComparer.Ordinal);
        }

        public async Task<AppointmentDto?> GetAppointmentAsync(int userId, string appointmentId, CancellationToken ct)
        {
            string patientId = await ResolvePatientIdAsync(userId, ct);
            AppointmentDto? dto = await appointmentsService.GetByIdAsync(appointmentId, ct);
            if (dto is null || dto.PatientId != patientId)
            {
                return null;
            }

            return dto;
        }

        public async Task<bool> CancelAppointmentAsync(int userId, string appointmentId, string? reason, CancellationToken ct)
        {
            AppointmentDto? dto = await GetAppointmentAsync(userId, appointmentId, ct);
            if (dto is null)
            {
                return false;
            }

            logger.LogInformation("Paciente {UserId} canceló el turno {AppointmentId}", userId, appointmentId);
            return await appointmentsService.CancelAsync(appointmentId, userId, reason, ct);
        }

        public async Task<IEnumerable<MedicalHistoryDto>> GetHistoryAsync(int userId, CancellationToken ct)
        {
            string patientId = await ResolvePatientIdAsync(userId, ct);
            IEnumerable<MedicalHistoryDto> history = await medicalHistoryService.GetByPatientAsync(patientId, ct);
            return history.OrderByDescending(h => h.Date);
        }

        private async Task<string> ResolvePatientIdAsync(int userId, CancellationToken ct)
        {
            int? patientId = await patientIdentityService.GetPatientIdByUserIdAsync(userId, ct);
            if (!patientId.HasValue)
            {
                throw new NotFoundException("El usuario no tiene un paciente asociado.");
            }

            return patientId.Value.ToString();
        }
    }
}
