using System;
using System.Collections.Generic;
using System.Threading;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.DTOs.Response.Medical;

namespace DoctorWare.Services.Interfaces
{
    public interface IPatientPortalService
    {
        Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(int userId, DateTime? startDate, DateTime? endDate, CancellationToken ct);
        Task<AppointmentDto?> GetAppointmentAsync(int userId, string appointmentId, CancellationToken ct);
        Task<bool> CancelAppointmentAsync(int userId, string appointmentId, string? reason, CancellationToken ct);
        Task<IEnumerable<MedicalHistoryDto>> GetHistoryAsync(int userId, CancellationToken ct);
    }
}
