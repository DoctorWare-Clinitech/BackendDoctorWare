using DoctorWare.DTOs.Response.Frontend;
using DoctorWare.Models;

namespace DoctorWare.Mappers
{
    public static class UserMapper
    {
        public static UserFrontendDto ToFrontendDto(USUARIOS user, PERSONAS? persona, string role)
        {
            return new UserFrontendDto
            {
                Id = user.ID_USUARIOS.ToString(),
                Email = user.EMAIL,
                Name = persona is null ? string.Empty : ($"{persona.NOMBRE} {persona.APELLIDO}").Trim(),
                Role = role,
                Status = user.ACTIVO ? "active" : "inactive",
                Phone = persona?.TELEFONO_PRINCIPAL,
                Avatar = null,
                CreatedAt = user.FECHA_CREACION,
                UpdatedAt = user.ULTIMA_ACTUALIZACION
            };
        }
    }
}

