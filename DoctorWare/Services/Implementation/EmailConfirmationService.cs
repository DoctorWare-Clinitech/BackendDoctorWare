using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Response;
using DoctorWare.Models;
using DoctorWare.Repositories.Interfaces;
using DoctorWare.Services.Interfaces;
using DoctorWare.Services.Templates;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Services.Implementation
{
    public class EmailConfirmationService : IEmailConfirmationService
    {
        private readonly IUsuariosRepository usuariosRepository;
        private readonly IPersonasRepository personasRepository;
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration configuration;
        private readonly ILogger<EmailConfirmationService> logger;

        public EmailConfirmationService(
            IUsuariosRepository usuariosRepository,
            IPersonasRepository personasRepository,
            IDbConnectionFactory connectionFactory,
            IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<EmailConfirmationService> logger)
        {
            this.usuariosRepository = usuariosRepository;
            this.personasRepository = personasRepository;
            this.connectionFactory = connectionFactory;
            this.emailSender = emailSender;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task SendConfirmationEmailAsync(int userId, CancellationToken cancellationToken = default, bool bypassCooldown = false)
        {
            var user = await usuariosRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("No se encontró el usuario {UserId} al intentar enviar confirmación de email", userId);
                return;
            }

            if (user.EMAIL_CONFIRMADO)
            {
                return;
            }

            var persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
            var status = await IssueTokenAndSendAsync(user, persona, bypassCooldown, cancellationToken);
            if (status == ResendConfirmationStatus.CooldownActive)
            {
                logger.LogDebug("Cooldown activo al intentar enviar confirmación para el usuario {UserId}", userId);
            }
        }

        public async Task<EmailConfirmationResult> ConfirmAsync(int userId, string token, CancellationToken cancellationToken = default)
        {
            var user = await usuariosRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return EmailConfirmationResult.NotFound();
            }

            if (user.EMAIL_CONFIRMADO)
            {
                return EmailConfirmationResult.AlreadyConfirmed();
            }

            if (string.IsNullOrWhiteSpace(user.TOKEN_RECUPERACION) || user.TOKEN_EXPIRACION is null)
            {
                return EmailConfirmationResult.InvalidToken();
            }

            var providedHash = HashToken(token);
            if (!string.Equals(user.TOKEN_RECUPERACION, providedHash, StringComparison.Ordinal))
            {
                return EmailConfirmationResult.InvalidToken();
            }

            var tokenMinutes = GetTokenMinutes();
            if (tokenMinutes > 0 && DateTime.UtcNow > user.TOKEN_EXPIRACION.Value)
            {
                return EmailConfirmationResult.Expired();
            }

            using var con = connectionFactory.CreateConnection();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                const string updateSql = @"update public.""USUARIOS"" 
set ""EMAIL_CONFIRMADO""=true, ""TOKEN_RECUPERACION""='', ""TOKEN_EXPIRACION""=NULL, ""ULTIMA_ACTUALIZACION""=@now,
    ""ID_ESTADOS_USUARIO"" = coalesce((select ""ID_ESTADOS_USUARIO"" from public.""ESTADOS_USUARIO"" where ""NOMBRE""='Activo' limit 1), ""ID_ESTADOS_USUARIO"")
where ""ID_USUARIOS""=@id";

                await con.ExecuteAsync(updateSql, new { id = userId, now = DateTime.UtcNow }, tx);
                tx.Commit();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                logger.LogError(ex, "Error actualizando confirmación de email para el usuario {UserId}", userId);
                throw;
            }

            return EmailConfirmationResult.Success();
        }

        public async Task<ResendConfirmationResult> ResendAsync(string email, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await usuariosRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (user is null)
            {
                return ResendConfirmationResult.NotFound();
            }

            if (user.EMAIL_CONFIRMADO)
            {
                return ResendConfirmationResult.AlreadyConfirmed();
            }

            var persona = await personasRepository.GetByIdAsync(user.ID_PERSONAS, cancellationToken);
            var status = await IssueTokenAndSendAsync(user, persona, bypassCooldown: false, cancellationToken);
            return status switch
            {
                ResendConfirmationStatus.Sent => ResendConfirmationResult.Sent(),
                ResendConfirmationStatus.CooldownActive => ResendConfirmationResult.CooldownActive(),
                ResendConfirmationStatus.AlreadyConfirmed => ResendConfirmationResult.AlreadyConfirmed(),
                _ => ResendConfirmationResult.NotFound()
            };
        }

        private async Task<ResendConfirmationStatus> IssueTokenAndSendAsync(USUARIOS user, PERSONAS? persona, bool bypassCooldown, CancellationToken cancellationToken)
        {
            if (user.EMAIL_CONFIRMADO)
            {
                return ResendConfirmationStatus.AlreadyConfirmed;
            }

            var tokenMinutes = GetTokenMinutes();
            var cooldownSeconds = GetCooldownSeconds();

            if (!bypassCooldown && IsCooldownActive(user, tokenMinutes, cooldownSeconds))
            {
                return ResendConfirmationStatus.CooldownActive;
            }

            var now = DateTime.UtcNow;
            var (tokenValue, tokenHash) = GenerateTokenPair();
            var expiresAt = tokenMinutes > 0 ? now.AddMinutes(tokenMinutes) : now;

            using (var con = connectionFactory.CreateConnection())
            {
                await con.ExecuteAsync(
                    "update public.\"USUARIOS\" set \"TOKEN_RECUPERACION\"=@token, \"TOKEN_EXPIRACION\"=@exp, \"ULTIMA_ACTUALIZACION\"=@now where \"ID_USUARIOS\"=@id",
                    new { token = tokenHash, exp = expiresAt, now, id = user.ID_USUARIOS });
            }

            user.TOKEN_RECUPERACION = tokenHash;
            user.TOKEN_EXPIRACION = expiresAt;
            user.ULTIMA_ACTUALIZACION = now;

            var confirmationUrl = BuildConfirmationUrl(user.ID_USUARIOS, tokenValue);
            var greetingName = persona is null
                ? string.Empty
                : $"{persona.NOMBRE} {persona.APELLIDO}".Trim();
            var htmlBody = EmailTemplates.BuildConfirmationEmail(greetingName, confirmationUrl);

            try
            {
                await emailSender.SendEmailAsync(user.EMAIL, "Confirma tu correo - DoctorWare", htmlBody, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enviando email de confirmación a {Email}", user.EMAIL);
            }

            return ResendConfirmationStatus.Sent;
        }

        private static bool IsCooldownActive(USUARIOS user, int tokenMinutes, int cooldownSeconds)
        {
            if (cooldownSeconds <= 0)
            {
                return false;
            }

            if (user.TOKEN_EXPIRACION is null)
            {
                return false;
            }

            var issuedAt = tokenMinutes > 0
                ? user.TOKEN_EXPIRACION.Value.AddMinutes(-tokenMinutes)
                : user.TOKEN_EXPIRACION.Value;

            return (DateTime.UtcNow - issuedAt).TotalSeconds < cooldownSeconds;
        }

        private (string tokenValue, string tokenHash) GenerateTokenPair()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var tokenValue = WebEncoders.Base64UrlEncode(tokenBytes);
            var tokenHash = HashToken(tokenValue);
            return (tokenValue, tokenHash);
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
        }

        private string BuildConfirmationUrl(int userId, string token)
        {
            var frontUrl = configuration["EmailConfirmation:FrontendConfirmUrl"];
            var baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
            var backendUrl = $"{baseUrl.TrimEnd('/')}/api/auth/confirm-email?uid={userId}&token={token}";

            if (!string.IsNullOrWhiteSpace(frontUrl))
            {
                var separator = frontUrl.Contains('?') ? '&' : '?';
                return $"{frontUrl}{separator}uid={userId}&token={token}";
            }

            return backendUrl;
        }

        private int GetTokenMinutes()
        {
            return int.TryParse(configuration["EmailConfirmation:TokenMinutes"], out var minutes) ? minutes : 60;
        }

        private int GetCooldownSeconds()
        {
            return int.TryParse(configuration["EmailConfirmation:ResendCooldownSeconds"], out var seconds) ? seconds : 60;
        }
    }
}

