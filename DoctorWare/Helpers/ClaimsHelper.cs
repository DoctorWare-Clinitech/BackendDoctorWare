using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DoctorWare.Helpers
{
    public static class ClaimsHelper
    {
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            string? sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? principal.FindFirst("sub")?.Value;

            int id;
            if (int.TryParse(sub, out id))
            {
                return id;
            }
            return null;
        }

        public static string? GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value
                   ?? principal.FindFirst("role")?.Value;
        }
    }
}
