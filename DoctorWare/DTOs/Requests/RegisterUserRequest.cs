using System.ComponentModel.DataAnnotations;

namespace DoctorWare.DTOs.Requests
{
    public class RegisterUserRequest
    {
        [Required]
        [MinLength(2)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        [Range(1, long.MaxValue)]
        public long NroDocumento { get; set; }

        [Required]
        public string TipoDocumentoCodigo { get; set; } = "DNI";

        [Required]
        public string Genero { get; set; } = "Prefiere no decirlo";
    }
}
