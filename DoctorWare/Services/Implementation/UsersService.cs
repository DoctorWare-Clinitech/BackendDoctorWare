using Dapper;
using DoctorWare.Constants;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests;
using DoctorWare.DTOs.Response;
using DoctorWare.Exceptions;
using DoctorWare.Models;
using DoctorWare.Repositories.Interfaces;
using DoctorWare.Services.Interfaces;
using DoctorWare.Utils;
using System.Data;

namespace DoctorWare.Services.Implementation
{
    public class UsersService : BaseService<USUARIOS, int>, IUserService
    {
        private readonly IUsuariosRepository usuariosRepository;
        private readonly IPersonasRepository personasRepository;
        private readonly IDbConnectionFactory factory;
        private readonly IEmailConfirmationService emailConfirmationService;

        public UsersService(
            IUsuariosRepository usuariosRepository,
            IPersonasRepository personasRepository,
            IDbConnectionFactory factory,
            IEmailConfirmationService emailConfirmationService)
            : base(usuariosRepository)
        {
            this.usuariosRepository = usuariosRepository;
            this.personasRepository = personasRepository;
            this.factory = factory;
            this.emailConfirmationService = emailConfirmationService;
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

                tx.Commit();

                await emailConfirmationService.SendConfirmationEmailAsync(usuarioCreado.ID_USUARIOS, cancellationToken, bypassCooldown: true);

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

        public async Task<string> ResolvePrimaryRoleAsync(int userId, CancellationToken cancellationToken = default)
        {
            var rawRole = await usuariosRepository.GetLatestRoleCodeAsync(userId, cancellationToken);
            if (string.IsNullOrWhiteSpace(rawRole))
            {
                return "patient";
            }

            return rawRole.Trim().ToLowerInvariant() switch
            {
                "profesional" or "professional" => "professional",
                "secretario" or "secretaria" or "secretary" => "secretary",
                "paciente" or "patient" => "patient",
                "admin" or "administrador" or "administradora" => "admin",
                _ => "patient"
            };
        }
    }
}
