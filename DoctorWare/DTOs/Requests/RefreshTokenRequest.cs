using System.ComponentModel.DataAnnotations;

namespace DoctorWare.DTOs.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

