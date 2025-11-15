using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;

namespace DoctorWare.Services.Interfaces
{
    public interface IMedicationsService
    {
        Task<IEnumerable<MedicationDto>> GetByPatientAsync(string patientId, CancellationToken ct);
        Task<MedicationDto> CreateAsync(CreateMedicationRequest request, int? createdByUserId, CancellationToken ct);
        Task<MedicationDto> UpdateAsync(string id, UpdateMedicationRequest request, CancellationToken ct);
        Task<MedicationDto> DiscontinueAsync(string id, CancellationToken ct);
    }
}

