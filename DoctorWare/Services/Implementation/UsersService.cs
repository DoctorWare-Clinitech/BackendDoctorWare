using Dapper;
using DoctorWare.Constants;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests;
using DoctorWare.DTOs.Response;
using DoctorWare.Exceptions;
using DoctorWare.Models;
using DoctorWare.Repositories.Interfaces;
using DoctorWare.Utils;
using System.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace DoctorWare.Services.Implementation
{
    public class UsersService : BaseService<USUARIOS, int>, DoctorWare.Services.Interfaces.IUserService
    {
        private readonly IUsuariosRepository usuariosRepository;
        private readonly IPersonasRepository personasRepository;
        private readonly IDbConnectionFactory factory;
        private readonly DoctorWare.Services.Interfaces.IEmailSender emailSender;
        private readonly IConfiguration configuration;
        private readonly ILogger<UsersService> logger;

        public UsersService(IUsuariosRepository usuariosRepository, IPersonasRepository personasRepository, IDbConnectionFactory factory, DoctorWare.Services.Interfaces.IEmailSender emailSender, IConfiguration configuration, ILogger<UsersService> logger)
            : base(usuariosRepository)
        {
            this.usuariosRepository = usuariosRepository;
            this.personasRepository = personasRepository;
            this.factory = factory;
            this.emailSender = emailSender;
            this.configuration = configuration;
            this.logger = logger;
        }

        protected override async Task ValidateAsync(USUARIOS entity)
        {
            // ValidaciÃ³n simple para evitar emails duplicados en creaciones directas
            var existing = await usuariosRepository.GetByEmailAsync(entity.EMAIL);
            if (existing != null)
            {
                throw new BadRequestException(ErrorMessages.EMAIL_ALREADY_REGISTERED);
            }
        }

        public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existing = await usuariosRepository.GetByEmailAsync(email, cancellationToken);
            if (existing != null)
            {
                throw new BadRequestException(ErrorMessages.EMAIL_ALREADY_REGISTERED);
            }

            using IDbConnection con = factory.CreateConnection();
            con.Open();
            using IDbTransaction tx = con.BeginTransaction();

            try
            {

                int tipoDocId = await personasRepository.GetTipoDocumentoIdByCodigoAsync(request.TipoDocumentoCodigo, con, tx, cancellationToken);
                int generoId = await personasRepository.GetGeneroIdByNombreAsync(request.Genero, con, tx, cancellationToken);
                DateTime now = DateTime.UtcNow;

                var persona = new PERSONAS
                {
                    NRO_DOCUMENTO = request.NroDocumento,
                    NOMBRE = request.Nombre,
                    APELLIDO = request.Apellido,
                    EMAIL_PRINCIPAL = email,
                    TELEFONO_PRINCIPAL = request.Telefono ?? string.Empty,
                    ACTIVO = true,
                    ID_TIPOS_DOCUMENTO = tipoDocId,
                    ID_GENEROS = generoId,
                    FECHA_CREACION = now,
                    ULTIMA_ACTUALIZACION = now
                };

                var personaCreada = await personasRepository.InsertWithConnectionAsync(persona, con, tx, cancellationToken);

                int estadoUsuarioId = await con.ExecuteScalarAsync<int?>(
                    "select \"ID_ESTADOS_USUARIO\" from public.\"ESTADOS_USUARIO\" where \"NOMBRE\" = @nombre limit 1",
                    new { nombre = "Pendiente de ConfirmaciÃ³n" }, tx) ??
                    await con.ExecuteScalarAsync<int>(
                        "select \"ID_ESTADOS_USUARIO\" from public.\"ESTADOS_USUARIO\" where \"NOMBRE\" = @nombre limit 1",
                        new { nombre = "Activo" }, tx);

                // Insertar USUARIOS
                USUARIOS usuario = new USUARIOS
                {
                    EMAIL = email,
                    PASSWORD_HASH = PasswordHasher.Hash(request.Password),
                    EMAIL_CONFIRMADO = false,
                    TELEFONO_CONFIRMADO = false,
                    FECHA_ULTIMO_ACCESO = null,
                    INTENTOS_FALLIDOS = 0,
                    BLOQUEADO_HASTA = null,
                    TOKEN_RECUPERACION = string.Empty,
                    TOKEN_EXPIRACION = null,
                    ACTIVO = true,
                    ID_PERSONAS = personaCreada.ID_PERSONAS,
                    ID_ESTADOS_USUARIO = estadoUsuarioId,
                    FECHA_CREACION = now,
                    ULTIMA_ACTUALIZACION = now
                };

                var usuarioCreado = await usuariosRepository.InsertWithConnectionAsync(usuario, con, tx, cancellationToken);

                // Generar token de confirmación de email y guardar dentro de la transacción
                var tokenBytes = RandomNumberGenerator.GetBytes(32);
                var emailToken = WebEncoders.Base64UrlEncode(tokenBytes);
                var minutes = int.TryParse(configuration["EmailConfirmation:TokenMinutes"], out var m) ? m : 60;
                DateTime expiresAt = DateTime.UtcNow.AddMinutes(minutes);

                string sqlUpdateToken = "update public.\"USUARIOS\" set \"TOKEN_RECUPERACION\"=@token, \"TOKEN_EXPIRACION\"=@exp where \"ID_USUARIOS\"=@id";
                await con.ExecuteAsync(sqlUpdateToken, new { token = emailToken, exp = expiresAt, id = usuarioCreado.ID_USUARIOS }, tx);

                tx.Commit();

                // Construir URL de confirmación (preferir front si está configurado)
                string? frontUrl = configuration["EmailConfirmation:FrontendConfirmUrl"];
                string baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
                string backendUrl = $"{baseUrl.TrimEnd('/')}/api/auth/confirm-email?uid={usuarioCreado.ID_USUARIOS}&token={emailToken}";
                string confirmationUrl = !string.IsNullOrWhiteSpace(frontUrl)
                    ? $"{frontUrl}{(frontUrl!.Contains('?') ? '&' : '?')}uid={usuarioCreado.ID_USUARIOS}&token={emailToken}"
                    : backendUrl;

                // Intentar enviar email (no fallar el registro si falla el envío)
                try
                {
                    string subject = "Confirma tu correo - DoctorWare";
                    string html = $@"<p>Hola {personaCreada.NOMBRE},</p>
                                     <p>Gracias por registrarte en DoctorWare. Por favor confirma tu correo haciendo click en el siguiente enlace:</p>
                                     <p><a href=""{confirmationUrl}"">Confirmar correo</a></p>
                                     <p>Si no fuiste tú, ignora este mensaje.</p>";
                    await emailSender.SendEmailAsync(email, subject, html, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "No se pudo enviar el email de confirmación a {Email}", email);
                }

                return new UserDto
                {
                    IdUser = usuarioCreado.ID_USUARIOS,
                    Email = usuarioCreado.EMAIL,
                    IdPersonas = usuarioCreado.ID_PERSONAS,
                    Activo = usuarioCreado.ACTIVO,
                    NombreCompleto = string.Concat(personaCreada.NOMBRE, " ", personaCreada.APELLIDO).Trim(),
                    Telefono = personaCreada.TELEFONO_PRINCIPAL,
                    FechaCreacion = usuarioCreado.FECHA_CREACION
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}


