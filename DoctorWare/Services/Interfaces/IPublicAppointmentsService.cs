using DoctorWare.DTOs.Requests.Public;
using DoctorWare.DTOs.Response.Appointments;

namespace DoctorWare.Services.Interfaces
{
    public interface IPublicAppointmentsService
    {
        Task<AppointmentDto> RequestAppointmentAsync(PublicAppointmentRequest request, CancellationToken ct);
    }
}
