using DoctorWare.Models;

namespace DoctorWare.Services.Interfaces
{
    public interface ITokenService
    {
        (string accessToken, DateTime expiresAt) GenerateAccessToken(USUARIOS user, string? fullName = null, string? role = null);
        (string refreshToken, DateTime expiresAt) GenerateRefreshToken(USUARIOS user, string? role = null);
        int GetAccessTokenTtlSeconds();
    }
}
