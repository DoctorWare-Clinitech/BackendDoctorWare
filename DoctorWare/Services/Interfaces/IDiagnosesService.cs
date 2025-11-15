using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;

namespace DoctorWare.Services.Interfaces
{
    public interface IDiagnosesService
    {
        Task<IEnumerable<DiagnosisDto>> GetByPatientAsync(string patientId, CancellationToken ct);
        Task<DiagnosisDto> CreateAsync(CreateDiagnosisRequest request, int? createdByUserId, CancellationToken ct);
        Task<DiagnosisDto> UpdateAsync(string id, UpdateDiagnosisRequest request, CancellationToken ct);
    }
}

