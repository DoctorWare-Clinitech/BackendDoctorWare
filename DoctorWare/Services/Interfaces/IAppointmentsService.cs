using DoctorWare.DTOs.Requests.Appointments;
using DoctorWare.DTOs.Response.Appointments;

namespace DoctorWare.Services.Interfaces
{
    public interface IAppointmentsService
    {
        Task<IEnumerable<AppointmentDto>> GetAsync(
            string? professionalUserId,
            string? patientId,
            DateTime? startDate,
            DateTime? endDate,
            string? status,
            string? type,
            CancellationToken ct);

        Task<AppointmentDto?> GetByIdAsync(string id, CancellationToken ct);

        Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, int? createdByUserId, CancellationToken ct);

        Task<AppointmentDto> UpdateAsync(string id, UpdateAppointmentRequest request, int? updatedByUserId, CancellationToken ct);

        Task<bool> CancelAsync(string id, int? cancelledByUserId, string? reason, CancellationToken ct);

        Task<(int total, int scheduled, int confirmed, int completed, int cancelled, int noShow)> GetStatsAsync(string? professionalUserId, CancellationToken ct);
    }
}

