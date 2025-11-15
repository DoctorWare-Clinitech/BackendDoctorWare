using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.Models;
using System.Data;

namespace DoctorWare.Repositories.Implementation
{
    public class UsuariosRepository(IDbConnectionFactory factory) : BaseRepository<USUARIOS, int>(factory), Interfaces.IUsuariosRepository
    {
        protected override string Table => "public.\"USUARIOS\"";
        protected override string Key => "\"ID_USUARIOS\"";

        protected override string SelectColumns => "\"ID_USUARIOS\", \"EMAIL\", \"PASSWORD_HASH\", \"EMAIL_CONFIRMADO\", \"TELEFONO_CONFIRMADO\", \"FECHA_ULTIMO_ACCESO\", \"INTENTOS_FALLIDOS\", \"BLOQUEADO_HASTA\", \"TOKEN_RECUPERACION\", \"TOKEN_EXPIRACION\", \"ACTIVO\", \"ID_PERSONAS\", \"ID_ESTADOS_USUARIO\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\"";

        protected override object ToDb(USUARIOS entity)
        {
            return new
            {
                entity.EMAIL,
                entity.PASSWORD_HASH,
                entity.EMAIL_CONFIRMADO,
                entity.TELEFONO_CONFIRMADO,
                entity.FECHA_ULTIMO_ACCESO,
                entity.INTENTOS_FALLIDOS,
                entity.BLOQUEADO_HASTA,
                entity.TOKEN_RECUPERACION,
                entity.TOKEN_EXPIRACION,
                entity.ACTIVO,
                entity.ID_PERSONAS,
                entity.ID_ESTADOS_USUARIO,
                entity.FECHA_CREACION,
                entity.ULTIMA_ACTUALIZACION
            };
        }

        protected override string InsertSql =>
            $"insert into {Table} (\"EMAIL\", \"PASSWORD_HASH\", \"EMAIL_CONFIRMADO\", \"TELEFONO_CONFIRMADO\", \"FECHA_ULTIMO_ACCESO\", \"INTENTOS_FALLIDOS\", \"BLOQUEADO_HASTA\", \"TOKEN_RECUPERACION\", \"TOKEN_EXPIRACION\", \"ACTIVO\", \"ID_PERSONAS\", \"ID_ESTADOS_USUARIO\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") " +
            "values (@EMAIL, @PASSWORD_HASH, @EMAIL_CONFIRMADO, @TELEFONO_CONFIRMADO, @FECHA_ULTIMO_ACCESO, @INTENTOS_FALLIDOS, @BLOQUEADO_HASTA, @TOKEN_RECUPERACION, @TOKEN_EXPIRACION, @ACTIVO, @ID_PERSONAS, @ID_ESTADOS_USUARIO, @FECHA_CREACION, @ULTIMA_ACTUALIZACION) " +
            $"returning {SelectColumns}";

        protected override string UpdateSql =>
            $"update {Table} set \"EMAIL\"=@EMAIL, \"PASSWORD_HASH\"=@PASSWORD_HASH, \"EMAIL_CONFIRMADO\"=@EMAIL_CONFIRMADO, \"TELEFONO_CONFIRMADO\"=@TELEFONO_CONFIRMADO, \"FECHA_ULTIMO_ACCESO\"=@FECHA_ULTIMO_ACCESO, \"INTENTOS_FALLIDOS\"=@INTENTOS_FALLIDOS, \"BLOQUEADO_HASTA\"=@BLOQUEADO_HASTA, \"TOKEN_RECUPERACION\"=@TOKEN_RECUPERACION, \"TOKEN_EXPIRACION\"=@TOKEN_EXPIRACION, \"ACTIVO\"=@ACTIVO, \"ID_PERSONAS\"=@ID_PERSONAS, \"ID_ESTADOS_USUARIO\"=@ID_ESTADOS_USUARIO, \"ULTIMA_ACTUALIZACION\"=@ULTIMA_ACTUALIZACION where \"ID_USUARIOS\"=@ID_USUARIOS";

        public async Task<USUARIOS?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            string sql = $"select {SelectColumns} from {Table} where \"EMAIL\" = @email";
            return await con.QuerySingleOrDefaultAsync<USUARIOS>(sql, new { email });
        }

        public async Task<USUARIOS> InsertWithConnectionAsync(USUARIOS entity, IDbConnection con, IDbTransaction tx, CancellationToken cancellationToken = default)
        {
            return await con.QuerySingleAsync<USUARIOS>(InsertSql, ToDb(entity), tx);
        }

        public async Task<string?> GetLatestRoleCodeAsync(int userId, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            const string sql = @"select r.""NOMBRE"" as Nombre
                from public.""USUARIOS_ROLES"" ur
                join public.""ROLES"" r on r.""ID_ROLES"" = ur.""ID_ROLES""
                where ur.""ID_USUARIOS"" = @userId
                order by ur.""FECHA_CREACION"" desc
                limit 1";

            string? result = await con.QueryFirstOrDefaultAsync<string?>(new CommandDefinition(sql, new { userId }, cancellationToken: cancellationToken));
            return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
        }

        public async Task<USUARIOS?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            string sql = $"select {SelectColumns} from {Table} where \"TOKEN_RECUPERACION\" = @token limit 1";
            return await con.QuerySingleOrDefaultAsync<USUARIOS>(new CommandDefinition(sql, new { token = tokenHash }, cancellationToken: cancellationToken));
        }
    }
}
