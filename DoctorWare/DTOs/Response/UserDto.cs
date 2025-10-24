namespace DoctorWare.DTOs.Response
{
    public class UserDto
    {
        public int IdUser { get; set; }

        public string Email { get; set; } = string.Empty;

        public int IdPersonas { get; set; }

        public bool Activo { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}

