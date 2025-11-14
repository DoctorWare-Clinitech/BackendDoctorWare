using System.ComponentModel.DataAnnotations;

namespace DoctorWare.DTOs.Requests
{
    public class RegisterProfessionalRequest : RegisterUserRequest
    {
        [Required]
        [MinLength(5)]
        public string MatriculaNacional { get; set; } = string.Empty;

        [Required]
        [MinLength(5)]
        public string MatriculaProvincial { get; set; } = string.Empty;

        [Required]
        public int EspecialidadId { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Universidad { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{11}$")]
        public string CUIT_CUIL { get; set; } = string.Empty;
    }
}

