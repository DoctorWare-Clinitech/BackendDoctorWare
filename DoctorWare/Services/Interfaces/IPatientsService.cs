using DoctorWare.DTOs.Requests.Patients;
using DoctorWare.DTOs.Response.Patients;

namespace DoctorWare.Services.Interfaces
{
    public interface IPatientsService
    {
        Task<IEnumerable<PatientDto>> GetAsync(string? name, string? dni, string? email, string? phone, string? professionalUserId, bool? isActive, CancellationToken ct);
        Task<PatientDto?> GetByIdAsync(string id, CancellationToken ct);
        Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken ct);
        Task<PatientDto> UpdateAsync(string id, UpdatePatientRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(string id, CancellationToken ct);
        Task<IEnumerable<PatientSummaryDto>> GetSummaryAsync(string? professionalUserId, CancellationToken ct);
    }
}

