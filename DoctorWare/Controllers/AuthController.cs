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
        private readonly IEmailConfirmationService emailConfirmationService;
        private readonly ILogger<AuthController> logger;

        public AuthController(
            IUserService userService,
            IUsuariosRepository usuariosRepository,
            IPersonasRepository personasRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            IEmailConfirmationService emailConfirmationService,
            ILogger<AuthController> logger)
        {
            this.userService = userService;
            this.usuariosRepository = usuariosRepository;
            this.personasRepository = personasRepository;
            this.tokenService = tokenService;
            this.configuration = configuration;
            this.emailConfirmationService = emailConfirmationService;
            this.logger = logger;
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
                throw new UnauthorizedAccessException(ErrorMessages.INVALID_CREDENTIALS);
            }

            if (!DoctorWare.Utils.PasswordHasher.Verify(request.Password, user.PASSWORD_HASH))
            {
                throw new UnauthorizedAccessException(ErrorMessages.INVALID_CREDENTIALS);
            }

            if (!user.EMAIL_CONFIRMADO)
            {
                throw new UnauthorizedAccessException(ErrorMessages.EMAIL_NOT_CONFIRMED);
            }

            var persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
            string fullName = persona is null ? string.Empty : $"{persona.NOMBRE} {persona.APELLIDO}".Trim();
            string role = await userService.ResolvePrimaryRoleAsync(user.ID_USUARIOS, cancellationToken);

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
                    throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN);
                }

                string? sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var userId))
                {
                    throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN);
                }

                Models.USUARIOS? user = await usuariosRepository.GetByIdAsync(userId, cancellationToken);
                if (user is null)
                {
                    throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN);
                }

                if (!user.EMAIL_CONFIRMADO)
                {
                    throw new UnauthorizedAccessException(ErrorMessages.EMAIL_NOT_CONFIRMED);
                }

                Models.PERSONAS? persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
                string fullName = persona is null ? string.Empty : $"{persona.NOMBRE} {persona.APELLIDO}".Trim();

                string role = await userService.ResolvePrimaryRoleAsync(user.ID_USUARIOS, cancellationToken);

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
                throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN);
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
                throw new DoctorWare.Exceptions.BadRequestException(ErrorMessages.BAD_REQUEST);
            }

            Models.USUARIOS user = await usuariosRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user is null)
            {
                throw new DoctorWare.Exceptions.NotFoundException(ErrorMessages.NOT_FOUND);
            }

            Models.PERSONAS persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
            string role = User.GetRole() ?? await userService.ResolvePrimaryRoleAsync(user.ID_USUARIOS, cancellationToken);
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
                throw new DoctorWare.Exceptions.BadRequestException(ErrorMessages.BAD_REQUEST);
            }

            var result = await emailConfirmationService.ConfirmAsync(uid, token, cancellationToken);

            if (!string.IsNullOrWhiteSpace(redirectBase))
            {
                return result.Status switch
                {
                    EmailConfirmationStatus.Success => Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=success"),
                    EmailConfirmationStatus.AlreadyConfirmed => Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=success&already=true"),
                    EmailConfirmationStatus.NotFound => Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=not_found"),
                    EmailConfirmationStatus.Expired => Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=expired_token"),
                    EmailConfirmationStatus.InvalidToken => Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=invalid_token"),
                    _ => Redirect($"{redirectBase}{(redirectBase.Contains('?') ? '&' : '?')}status=error&reason=error")
                };
            }

            return result.Status switch
            {
                EmailConfirmationStatus.Success => Ok(new { message = "Email confirmado" }),
                EmailConfirmationStatus.AlreadyConfirmed => Ok(new { message = "Email ya confirmado" }),
                EmailConfirmationStatus.NotFound => throw new DoctorWare.Exceptions.NotFoundException(ErrorMessages.NOT_FOUND),
                EmailConfirmationStatus.InvalidToken => throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN),
                EmailConfirmationStatus.Expired => throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN),
                _ => throw new UnauthorizedAccessException(ErrorMessages.INVALID_REFRESH_TOKEN)
            };
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
            var genericOk = Ok(new { message = "Si el email está registrado y no confirmado, enviaremos un enlace de confirmación." });

            var result = await emailConfirmationService.ResendAsync(email, cancellationToken);

            if (result.Status == ResendConfirmationStatus.CooldownActive)
            {
                logger.LogDebug("Cooldown activo al intentar reenviar confirmación para {Email}", email);
            }

            return genericOk;
        }
    }
}
