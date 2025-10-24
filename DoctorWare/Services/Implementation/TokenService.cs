using DoctorWare.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DoctorWare.Services.Implementation
{
    public class TokenService : DoctorWare.Services.Interfaces.ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Genera un access token JWT para el usuario.
        /// </summary>
        /// <param name="user">Entidad USUARIOS.</param>
        /// <param name="fullName">Nombre completo opcional.</param>
        /// <param name="role">Rol opcional para claims.</param>
        public (string accessToken, DateTime expiresAt) GenerateAccessToken(USUARIOS user, string? fullName = null, string? role = null)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var issuer = jwtSection["Issuer"] ?? "DoctorWare";
            var audience = jwtSection["Audience"] ?? "DoctorWare.Client";
            var minutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var m) ? m : 60;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(minutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.ID_USUARIOS.ToString()),
                new(JwtRegisteredClaimNames.Email, user.EMAIL),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString())
            };

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, fullName));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                // claim estándar de rol para compatibilidad; además un claim "role" simple para el front
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expires);
        }

        /// <summary>
        /// Genera un refresh token JWT para el usuario.
        /// </summary>
        /// <param name="user">Entidad USUARIOS.</param>
        /// <param name="role">Rol opcional para claims.</param>
        public (string refreshToken, DateTime expiresAt) GenerateRefreshToken(USUARIOS user, string? role = null)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var issuer = jwtSection["Issuer"] ?? "DoctorWare";
            var audience = jwtSection["Audience"] ?? "DoctorWare.Client";
            var minutes = int.TryParse(jwtSection["RefreshTokenMinutes"], out var m) ? m : 60 * 24 * 7;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(minutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.ID_USUARIOS.ToString()),
                new(JwtRegisteredClaimNames.Email, user.EMAIL),
                new("token_type", "refresh"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString())
            };

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expires);
        }

        /// <summary>
        /// Devuelve el TTL del access token en segundos.
        /// </summary>
        public int GetAccessTokenTtlSeconds()
        {
            var jwtSection = configuration.GetSection("Jwt");
            var minutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var m) ? m : 60;
            return minutes * 60;
        }
    }
}
