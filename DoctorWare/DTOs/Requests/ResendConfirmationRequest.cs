using System.ComponentModel.DataAnnotations;

namespace DoctorWare.DTOs.Requests
{
    public class ResendConfirmationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

