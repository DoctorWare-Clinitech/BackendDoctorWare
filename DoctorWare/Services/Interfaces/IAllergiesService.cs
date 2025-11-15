using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;

namespace DoctorWare.Services.Interfaces
{
    public interface IAllergiesService
    {
        Task<IEnumerable<AllergyDto>> GetByPatientAsync(string patientId, CancellationToken ct);
        Task<AllergyDto> CreateAsync(CreateAllergyRequest request, CancellationToken ct);
        Task<AllergyDto> UpdateAsync(string id, UpdateAllergyRequest request, CancellationToken ct);
        Task<bool> DeactivateAsync(string id, CancellationToken ct);
    }
}

