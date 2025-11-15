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
            // Validación simple para evitar emails duplicados en creaciones directas
            USUARIOS? existing = await usuariosRepository.GetByEmailAsync(entity.EMAIL);
            if (existing != null)
            {
                throw new BadRequestException(ErrorMessages.EMAIL_ALREADY_REGISTERED);
            }
        }

        public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            USUARIOS? existing = await usuariosRepository.GetByEmailAsync(email, cancellationToken);
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

                PERSONAS persona = new PERSONAS
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

                PERSONAS personaCreada = await personasRepository.InsertWithConnectionAsync(persona, con, tx, cancellationToken);

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

                USUARIOS usuarioCreado = await usuariosRepository.InsertWithConnectionAsync(usuario, con, tx, cancellationToken);

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
            string? rawRole = await usuariosRepository.GetLatestRoleCodeAsync(userId, cancellationToken);
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

        public async Task<UserDto> RegisterPatientAsync(RegisterPatientRequest request, CancellationToken ct)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            USUARIOS? existing = await usuariosRepository.GetByEmailAsync(email, ct);
            if (existing != null)
            {
                throw new BadRequestException(ErrorMessages.EMAIL_ALREADY_REGISTERED);
            }

            using IDbConnection con = factory.CreateConnection();
            con.Open();
            using IDbTransaction tx = con.BeginTransaction();

            try
            {
                int tipoDocId = await personasRepository.GetTipoDocumentoIdByCodigoAsync(request.TipoDocumentoCodigo, con, tx, ct);
                int generoId = await personasRepository.GetGeneroIdByNombreAsync(request.Genero, con, tx, ct);
                DateTime now = DateTime.UtcNow;

                PERSONAS persona = new PERSONAS
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

                PERSONAS personaCreada = await personasRepository.InsertWithConnectionAsync(persona, con, tx, ct);

                int estadoUsuarioId = await con.ExecuteScalarAsync<int?>(
                    "select \"ID_ESTADOS_USUARIO\" from public.\"ESTADOS_USUARIO\" where \"NOMBRE\" = @nombre limit 1",
                    new { nombre = "Pendiente de Confirmación" }, tx) ??
                    await con.ExecuteScalarAsync<int>(
                        "select \"ID_ESTADOS_USUARIO\" from public.\"ESTADOS_USUARIO\" where \"NOMBRE\" = @nombre limit 1",
                        new { nombre = "Activo" }, tx);

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

                USUARIOS usuarioCreado = await usuariosRepository.InsertWithConnectionAsync(usuario, con, tx, ct);

                // Vincular rol PACIENTE
                int? roleId = await con.ExecuteScalarAsync<int?>(
                    "select \"ID_ROLES\" from public.\"ROLES\" where upper(\"NOMBRE\") = upper(@name) limit 1",
                    new { name = "PACIENTE" }, tx);
                if (roleId.HasValue)
                {
                    await con.ExecuteAsync(
                        "insert into public.\"USUARIOS_ROLES\" (\"ID_USUARIOS\", \"ID_ROLES\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") values (@u, @r, @now, @now)",
                        new { u = usuarioCreado.ID_USUARIOS, r = roleId.Value, now }, tx);
                }

                // Crear registro de PACIENTES (campos opcionales)
                await con.ExecuteAsync(
                    "insert into public.\"PACIENTES\" (\"SEGURO_PROVEEDOR\", \"SEGURO_NUMERO_AFILIADO\", \"CONTACTO_EMERGENCIA_NOMBRE\", \"CONTACTO_EMERGENCIA_TELEFONO\", \"CONTACTO_EMERGENCIA_RELACION\", \"ACTIVO\", \"ID_PERSONAS\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") " +
                    "values (@obra, @nro, @cen, @cet, @cer, true, @idp, @now, @now)",
                    new
                    {
                        obra = request.ObraSocial,
                        nro = request.NumeroAfiliado,
                        cen = request.ContactoEmergenciaNombre,
                        cet = request.ContactoEmergenciaTelefono,
                        cer = request.ContactoEmergenciaRelacion,
                        idp = personaCreada.ID_PERSONAS,
                        now
                    }, tx);

                tx.Commit();

                await emailConfirmationService.SendConfirmationEmailAsync(usuarioCreado.ID_USUARIOS, ct, bypassCooldown: true);

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

        public async Task<UserDto> RegisterProfessionalAsync(RegisterProfessionalRequest request, CancellationToken ct)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            USUARIOS? existing = await usuariosRepository.GetByEmailAsync(email, ct);
            if (existing != null)
            {
                throw new BadRequestException(ErrorMessages.EMAIL_ALREADY_REGISTERED);
            }

            // Validaciones específicas
            if (!CUITValidator.IsValidCUIT(request.CUIT_CUIL))
            {
                throw new BadRequestException(ErrorMessages.CUIT_INVALID);
            }

            using IDbConnection con = factory.CreateConnection();
            con.Open();
            using IDbTransaction tx = con.BeginTransaction();

            try
            {
                // Unicidad de matrículas y CUIT
                int countMn = await con.ExecuteScalarAsync<int>(
                    "select count(1) from public.\"PROFESIONALES\" where upper(\"MATRICULA_NACIONAL\") = upper(@mn)",
                    new { mn = request.MatriculaNacional }, tx);
                if (countMn > 0)
                {
                    throw new BadRequestException(ErrorMessages.MATRICULA_NACIONAL_DUPLICADA);
                }

                int countMp = await con.ExecuteScalarAsync<int>(
                    "select count(1) from public.\"PROFESIONALES\" where upper(\"MATRICULA_PROVINCIAL\") = upper(@mp)",
                    new { mp = request.MatriculaProvincial }, tx);
                if (countMp > 0)
                {
                    throw new BadRequestException(ErrorMessages.MATRICULA_PROVINCIAL_DUPLICADA);
                }

                string cuit = CUITValidator.Normalize(request.CUIT_CUIL);
                int countCuit = await con.ExecuteScalarAsync<int>(
                    "select count(1) from public.\"PROFESIONALES\" where \"CUIT_CUIL\" = @cuit",
                    new { cuit }, tx);
                if (countCuit > 0)
                {
                    throw new BadRequestException(ErrorMessages.CUIT_DUPLICADO);
                }

                // Validar especialidad
                int? existsEsp = await con.ExecuteScalarAsync<int?>(
                    "select 1 from public.\"ESPECIALIDADES\" where \"ID_ESPECIALIDADES\"=@id limit 1",
                    new { id = request.EspecialidadId }, tx);
                if (!existsEsp.HasValue)
                {
                    throw new BadRequestException(ErrorMessages.ESPECIALIDAD_NO_ENCONTRADA);
                }

                int tipoDocId = await personasRepository.GetTipoDocumentoIdByCodigoAsync(request.TipoDocumentoCodigo, con, tx, ct);
                int generoId = await personasRepository.GetGeneroIdByNombreAsync(request.Genero, con, tx, ct);
                DateTime now = DateTime.UtcNow;

                PERSONAS persona = new PERSONAS
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

                PERSONAS personaCreada = await personasRepository.InsertWithConnectionAsync(persona, con, tx, ct);

                int estadoUsuarioId = await con.ExecuteScalarAsync<int?>(
                    "select \"ID_ESTADOS_USUARIO\" from public.\"ESTADOS_USUARIO\" where \"NOMBRE\" = @nombre limit 1",
                    new { nombre = "Pendiente de Confirmación" }, tx) ??
                    await con.ExecuteScalarAsync<int>(
                        "select \"ID_ESTADOS_USUARIO\" from public.\"ESTADOS_USUARIO\" where \"NOMBRE\" = @nombre limit 1",
                        new { nombre = "Activo" }, tx);

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

                USUARIOS usuarioCreado = await usuariosRepository.InsertWithConnectionAsync(usuario, con, tx, ct);

                // Vincular rol PROFESIONAL
                int? roleId = await con.ExecuteScalarAsync<int?>(
                    "select \"ID_ROLES\" from public.\"ROLES\" where upper(\"NOMBRE\") = upper(@name) limit 1",
                    new { name = "PROFESIONAL" }, tx);
                if (roleId.HasValue)
                {
                    await con.ExecuteAsync(
                        "insert into public.\"USUARIOS_ROLES\" (\"ID_USUARIOS\", \"ID_ROLES\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") values (@u, @r, @now, @now)",
                        new { u = usuarioCreado.ID_USUARIOS, r = roleId.Value, now }, tx);
                }

                // Crear registro en PROFESIONALES
                int profesionalId = await con.ExecuteScalarAsync<int>(
                    "insert into public.\"PROFESIONALES\" (\"MATRICULA_NACIONAL\", \"MATRICULA_PROVINCIAL\", \"TITULO\", \"UNIVERSIDAD\", \"ANIO_EGRESO\", \"CUIT_CUIL\", \"DURACION_TURNO_EN_MINUTOS\", \"ACTIVO\", \"ID_PERSONAS\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") " +
                    "values (@mn, @mp, @titulo, @uni, NULL, @cuit, @duracion, true, @idp, @now, @now) returning \"ID_PROFESIONALES\"",
                    new
                    {
                        mn = request.MatriculaNacional,
                        mp = request.MatriculaProvincial,
                        titulo = request.Titulo,
                        uni = request.Universidad,
                        cuit = cuit,
                        duracion = 30,
                        idp = personaCreada.ID_PERSONAS,
                        now
                    }, tx);

                // Relacionar especialidad
                await con.ExecuteAsync(
                    "insert into public.\"PROFESIONAL_ESPECIALIDADES\" (\"ID_PROFESIONALES\", \"ID_ESPECIALIDADES\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") values (@idprof, @idesp, @now, @now)",
                    new { idprof = profesionalId, idesp = request.EspecialidadId, now }, tx);

                tx.Commit();

                await emailConfirmationService.SendConfirmationEmailAsync(usuarioCreado.ID_USUARIOS, ct, bypassCooldown: true);

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
