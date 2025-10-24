using DoctorWare.DTOs.Requests;
using DoctorWare.DTOs.Response;
using DoctorWare.Models;

namespace DoctorWare.Services.Interfaces
{
    public interface IUserService : IService<USUARIOS, int>
    {
        Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    }
}

