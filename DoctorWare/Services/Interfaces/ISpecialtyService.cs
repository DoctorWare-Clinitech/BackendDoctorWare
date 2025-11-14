using DoctorWare.DTOs.Response;

namespace DoctorWare.Services.Interfaces
{
    public interface ISpecialtyService
    {
        Task<List<SpecialtyDto>> GetActiveSpecialtiesAsync(CancellationToken ct);
    }
}

