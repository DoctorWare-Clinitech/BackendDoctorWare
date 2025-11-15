using DoctorWare.DTOs.Response;

namespace DoctorWare.Services.Interfaces
{
    public class ProfessionalListItemDto
    {
        public int Id { get; set; }                 // ID_PROFESIONALES
        public string UserId { get; set; } = string.Empty; // ID_USUARIOS asociado, como string para frontend
        public string Name { get; set; } = string.Empty;    // Persona completa
        public string? MatriculaNacional { get; set; }
        public string? MatriculaProvincial { get; set; }
        public string? Especialidad { get; set; }
        public bool Activo { get; set; }
    }

    public interface IProfessionalsService
    {
        Task<IEnumerable<ProfessionalListItemDto>> GetAsync(int? specialtyId, string? name, CancellationToken ct);
        Task<ProfessionalListItemDto?> GetByIdAsync(int id, CancellationToken ct);
    }
}

