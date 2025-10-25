using Dapper;
using DoctorWare.Constants;
using DoctorWare.DTOs.Requests;
using DoctorWare.DTOs.Response;
using DoctorWare.DTOs.Response.Frontend;
using DoctorWare.Helpers;
using DoctorWare.Mappers;
using DoctorWare.Repositories.Interfaces;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace DoctorWare.Controllers
{
    /// <summary>
    /// Controlador de autenticacion. Gestiona credenciales y emision/renovacion de tokens.
    ///
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : BaseApiController
    {
        private readonly IUserService userService;
        private readonly IUsuariosRepository usuariosRepository;
        private readonly IPersonasRepository personasRepository;
        private readonly ITokenService tokenService;
        private readonly IConfiguration configuration;
        private readonly DoctorWare.Data.Interfaces.IDbConnectionFactory connectionFactory;
        private readonly ILogger<AuthController> logger;

        public AuthController(
            IUserService userService,
            IUsuariosRepository usuariosRepository,
            IPersonasRepository personasRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            DoctorWare.Data.Interfaces.IDbConnectionFactory connectionFactory,
            ILogger<AuthController> logger)
        {
            this.userService = userService;
            this.usuariosRepository = usuariosRepository;
            this.personasRepository = personasRepository;
            this.tokenService = tokenService;
            this.configuration = configuration;
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        private async Task<string?> GetUserRoleAsync(int userId, CancellationToken cancellationToken)
        {
            using var con = connectionFactory.CreateConnection();
            const string sql = @"select r.""NOMBRE"" as Nombre
                from public.""USUARIOS_ROLES"" ur
                join public.""ROLES"" r on r.""ID_ROLES"" = ur.""ID_ROLES""
                where ur.""ID_USUARIOS"" = @userId
                order by ur.""FECHA_CREACION"" desc
                limit 1";

            var nombre = await con.QueryFirstOrDefaultAsync<string?>(new CommandDefinition(sql, new { userId }, cancellationToken: cancellationToken));
            if (string.IsNullOrWhiteSpace(nombre)) return null;

            return nombre.Trim().ToLower() switch
            {
                "profesional" or "professional" => "professional",
                "secretario" or "secretaria" or "secretary" => "secretary",
                "paciente" or "patient" => "patient",
                "admin" or "administrador" or "administradora" => "admin",
                _ => "patient"
            };
        }

        /// <summary>
        /// Autentica credenciales y emite tokens de acceso y refresh.
        /// </summary>
        /// <param name="request">Credenciales: email y password.</param>
        /// <param name="cancellationToken">Token de cancelacion.</param>
        /// <returns>Objeto plano: token, refreshToken, user, expiresIn.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await usuariosRepository.GetByEmailAsync(email, cancellationToken);
            if (user is null)
            {
                return Unauthorized(new { message = ErrorMessages.INVALID_CREDENTIALS });
            }

            if (!DoctorWare.Utils.PasswordHasher.Verify(request.Password, user.PASSWORD_HASH))
            {
                return Unauthorized(new { message = ErrorMessages.INVALID_CREDENTIALS });
            }

            if (!user.EMAIL_CONFIRMADO)
            {
                return Unauthorized(new { message = ErrorMessages.EMAIL_NOT_CONFIRMED });
            }

            var persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
            string fullName = persona is null ? string.Empty : $"{persona.NOMBRE} {persona.APELLIDO}".Trim();
            string role = await GetUserRoleAsync(user.ID_USUARIOS, cancellationToken) ?? "patient";

            (string accessToken, System.DateTime _) = tokenService.GenerateAccessToken(user, fullName, role);
            (string refreshToken, System.DateTime _) = tokenService.GenerateRefreshToken(user, role);

            var userFrontend = UserMapper.ToFrontendDto(user, persona, role);

            return Ok(new
            {
                token = accessToken,
                refreshToken = refreshToken,
                user = userFrontend,
                expiresIn = tokenService.GetAccessTokenTtlSeconds()
            });
        }

        /// <summary>
        /// Registra un nuevo usuario y devuelve tokens + perfil (compatibilidad front).
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
        {
            UserDto created = await userService.RegisterAsync(request, cancellationToken);
            // Seguridad: no emitir tokens en registro. Devolver mensaje y flag.
            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Registro exitoso. Revisa tu correo para confirmar tu email.",
                requiresEmailConfirmation = true
            });
        }

        /// <summary>
        /// Refresca el par de tokens usando un refresh token vÃƒÂ¡lido.
        /// </summary>
        /// <param name="request">Refresh token vigente.</param>
        /// <param name="cancellationToken">Token de cancelaciÃƒÂ³n.</param>
        /// <returns>Nuevo par de tokens y los datos del usuario.</returns>       
        [HttpPost("refresh")]
        [Consumes("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            IConfigurationSection jwtSection = configuration.GetSection("Jwt");
            string secret = jwtSection["Secret"] ?? string.Empty;
            string issuer = jwtSection["Issuer"] ?? "DoctorWare";
            string audience = jwtSection["Audience"] ?? "DoctorWare.Client";

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(request.RefreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                string? typeClaim = principal.FindFirst("token_type")?.Value;
                if (!string.Equals(typeClaim, "refresh", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
                }

                string? sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var userId))
                {
                    return Unauthorized(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
                }

                Models.USUARIOS? user = await usuariosRepository.GetByIdAsync(userId, cancellationToken);
                if (user is null)
                {
                    return Unauthorized(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
                }

                if (!user.EMAIL_CONFIRMADO)
                {
                    return Unauthorized(new { message = ErrorMessages.EMAIL_NOT_CONFIRMED });
                }

                Models.PERSONAS? persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
                string fullName = persona is null ? string.Empty : $"{persona.NOMBRE} {persona.APELLIDO}".Trim();

                string role = await GetUserRoleAsync(user.ID_USUARIOS, cancellationToken) ?? "patient";

                (string accessToken, System.DateTime _) = tokenService.GenerateAccessToken(user, fullName, role);
                (string refreshToken, System.DateTime _) = tokenService.GenerateRefreshToken(user, role);

                var userFrontend = UserMapper.ToFrontendDto(user, persona, role);

                return Ok(new
                {
                    token = accessToken,
                    refreshToken = refreshToken,
                    user = userFrontend,
                    expiresIn = tokenService.GetAccessTokenTtlSeconds()
                });
            }
            catch
            {
                return Unauthorized(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado a partir del token de acceso.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelacion.</param>
        /// <returns>Datos del usuario autenticado.</returns>      
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserFrontendDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            int? userId = User.GetUserId();
            if (userId is null)
            {
                return BadRequest(new { message = ErrorMessages.BAD_REQUEST });
            }

            Models.USUARIOS user = await usuariosRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = ErrorMessages.NOT_FOUND });
            }

            Models.PERSONAS persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
            string role = User.GetRole() ?? await GetUserRoleAsync(user.ID_USUARIOS, cancellationToken) ?? "patient";
            var dto = UserMapper.ToFrontendDto(user, persona, role);
            return Ok(dto);
        }

        /// <summary>
        /// Confirma el correo electrónico del usuario mediante un token enviado por email.
        /// </summary>
        /// <param name="uid">ID del usuario.</param>
        /// <param name="token">Token de confirmación (Base64Url).</param>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] int uid, [FromQuery] string token, CancellationToken cancellationToken)
        {
            string? redirectBase = configuration["EmailConfirmation:BackendConfirmRedirectUrl"];

            if (uid <= 0 || string.IsNullOrWhiteSpace(token))
            {
                if (!string.IsNullOrWhiteSpace(redirectBase))
                    return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=bad_request");
                return BadRequest(new { message = ErrorMessages.BAD_REQUEST });
            }

            var user = await usuariosRepository.GetByIdAsync(uid, cancellationToken);
            if (user is null)
            {
                if (!string.IsNullOrWhiteSpace(redirectBase))
                    return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=not_found");
                return NotFound(new { message = ErrorMessages.NOT_FOUND });
            }

            if (user.EMAIL_CONFIRMADO)
            {
                if (!string.IsNullOrWhiteSpace(redirectBase))
                    return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=success&already=true");
                return Ok(new { message = "Email ya confirmado" });
            }

            if (string.IsNullOrWhiteSpace(user.TOKEN_RECUPERACION) || user.TOKEN_EXPIRACION is null)
            {
                if (!string.IsNullOrWhiteSpace(redirectBase))
                    return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=invalid_token");
                return BadRequest(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
            }

            // Comparar el hash del token (se guarda como SHA-256 hex en DB)
            var providedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
            if (!string.Equals(user.TOKEN_RECUPERACION, providedHash, StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(redirectBase))
                    return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=invalid_token");
                return BadRequest(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
            }

            // Si TokenMinutes <= 0, se interpreta como sin vencimiento
            int tokenMinutes = int.TryParse(configuration["EmailConfirmation:TokenMinutes"], out var tm) ? tm : 60;
            if (tokenMinutes > 0 && DateTime.UtcNow > user.TOKEN_EXPIRACION.Value)
            {
                if (!string.IsNullOrWhiteSpace(redirectBase))
                    return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=expired_token");
                return BadRequest(new { message = ErrorMessages.INVALID_REFRESH_TOKEN });
            }

            using var con = connectionFactory.CreateConnection();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                // Actualizar flags y limpiar token
                const string updateSql = @"update public.""USUARIOS"" 
set ""EMAIL_CONFIRMADO""=true, ""TOKEN_RECUPERACION""='', ""TOKEN_EXPIRACION""=NULL, ""ULTIMA_ACTUALIZACION""=@now,
    ""ID_ESTADOS_USUARIO"" = coalesce((select ""ID_ESTADOS_USUARIO"" from public.""ESTADOS_USUARIO"" where ""NOMBRE""='Activo' limit 1), ""ID_ESTADOS_USUARIO"")
where ""ID_USUARIOS""=@id";

                await con.ExecuteAsync(updateSql, new { id = uid, now = DateTime.UtcNow }, tx);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            if (!string.IsNullOrWhiteSpace(redirectBase))
                return Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=success");

            return Ok(new { message = "Email confirmado" });
        }

        /// <summary>
        /// Reenvía el email de confirmación si el usuario no está confirmado.
        /// La respuesta es siempre genérica para no revelar si el email existe.
        /// </summary>
        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        [Consumes("application/json")]
        public async Task<IActionResult> ResendConfirmation([FromBody] DoctorWare.DTOs.Requests.ResendConfirmationRequest request, CancellationToken cancellationToken)
        {
            string email = request.Email.Trim().ToLowerInvariant();
            var user = await usuariosRepository.GetByEmailAsync(email, cancellationToken);

            // Respuesta genérica por seguridad
            var genericOk = Ok(new { message = "Si el email está registrado y no confirmado, enviaremos un enlace de confirmación." });

            if (user is null || user.EMAIL_CONFIRMADO)
            {
                return genericOk;
            }

            try
            {
                int tokenMinutes = int.TryParse(configuration["EmailConfirmation:TokenMinutes"], out var m) ? m : 60;
                int cooldownSeconds = int.TryParse(configuration["EmailConfirmation:ResendCooldownSeconds"], out var cs) ? cs : 60;

                DateTime? expires = user.TOKEN_EXPIRACION;
                DateTime lastIssuedAt = DateTime.MinValue;
                if (expires.HasValue)
                {
                    lastIssuedAt = tokenMinutes > 0 ? expires.Value.AddMinutes(-tokenMinutes) : expires.Value; // cuando no expira, usamos expires como marca de emisión
                }
                if ((DateTime.UtcNow - lastIssuedAt).TotalSeconds < cooldownSeconds)
                {
                    return genericOk;
                }

                // Generar nuevo token + hash y guardar
                var tokenBytes = RandomNumberGenerator.GetBytes(32);
                var emailToken = WebEncoders.Base64UrlEncode(tokenBytes);
                var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(emailToken))).ToLowerInvariant();
                DateTime newExpires = tokenMinutes > 0 ? DateTime.UtcNow.AddMinutes(tokenMinutes) : DateTime.UtcNow; // sin vencimiento, guardamos marca de emisión

                using var con = connectionFactory.CreateConnection();
                await con.ExecuteAsync("update public.\"USUARIOS\" set \"TOKEN_RECUPERACION\"=@token, \"TOKEN_EXPIRACION\"=@exp, \"ULTIMA_ACTUALIZACION\"=@now where \"ID_USUARIOS\"=@id",
                    new { token = tokenHash, exp = newExpires, now = DateTime.UtcNow, id = user.ID_USUARIOS });

                // Construir URL
                string? frontUrl = configuration["EmailConfirmation:FrontendConfirmUrl"];
                string baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
                string backendUrl = $"{baseUrl.TrimEnd('/')}/api/auth/confirm-email?uid={user.ID_USUARIOS}&token={emailToken}";
                string confirmationUrl = !string.IsNullOrWhiteSpace(frontUrl)
                    ? $"{frontUrl}{(frontUrl!.Contains('?') ? '&' : '?')}uid={user.ID_USUARIOS}&token={emailToken}"
                    : backendUrl;

                string subject = "Confirma tu correo - DoctorWare";
                string html = $@"<p>Hola,</p>
                                 <p>Te enviamos nuevamente el enlace para confirmar tu correo en DoctorWare:</p>
                                 <p><a href=""{confirmationUrl}"">Confirmar correo</a></p>
                                 <p>Si no solicitaste esto, ignora este mensaje.</p>";

                try
                {
                    await HttpContext.RequestServices.GetRequiredService<DoctorWare.Services.Interfaces.IEmailSender>().SendEmailAsync(email, subject, html, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error reenviando el email de confirmación a {Email}", email);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en resend-confirmation para {Email}", email);
            }

            return genericOk;
        }
    }
}
