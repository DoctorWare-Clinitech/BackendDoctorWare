using System.ComponentModel.DataAnnotations;

namespace DoctorWare.DTOs.Requests
{
    public class RegisterPatientRequest : RegisterUserRequest
    {
        public string? ObraSocial { get; set; }

        public string? NumeroAfiliado { get; set; }

        public string? ContactoEmergenciaNombre { get; set; }

        public string? ContactoEmergenciaTelefono { get; set; }

        public string? ContactoEmergenciaRelacion { get; set; }
    }
}

