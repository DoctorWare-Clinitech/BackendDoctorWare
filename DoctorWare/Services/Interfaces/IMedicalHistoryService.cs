using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;

namespace DoctorWare.Services.Interfaces
{
    public interface IMedicalHistoryService
    {
        Task<IEnumerable<MedicalHistoryDto>> GetByPatientAsync(string patientId, CancellationToken ct);
        Task<MedicalHistoryDto?> GetByIdAsync(string id, CancellationToken ct);
        Task<MedicalHistoryDto> CreateAsync(CreateMedicalHistoryRequest request, int? createdByUserId, CancellationToken ct);
        Task<MedicalHistoryDto> UpdateAsync(string id, UpdateMedicalHistoryRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(string id, CancellationToken ct);
    }
}

