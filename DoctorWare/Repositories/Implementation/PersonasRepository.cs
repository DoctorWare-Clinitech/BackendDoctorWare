using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.Models;
using System.Data;

namespace DoctorWare.Repositories.Implementation
{
    public class PersonasRepository : BaseRepository<PERSONAS, int>, DoctorWare.Repositories.Interfaces.IPersonasRepository
    {
        public PersonasRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        protected override string Table => "public.\"PERSONAS\"";
        protected override string Key => "\"ID_PERSONAS\"";

        protected override string SelectColumns => "\"ID_PERSONAS\", \"NRO_DOCUMENTO\", \"NOMBRE\", \"APELLIDO\", \"FECHA_NACIMIENTO\", \"FOTO\", \"EMAIL_PRINCIPAL\", \"TELEFONO_PRINCIPAL\", \"CELULAR_PRINCIPAL\", \"CALLE\", \"NUMERO\", \"PISO\", \"DEPARTAMENTO\", \"LOCALIDAD\", \"PROVINCIA\", \"CODIGO_POSTAL\", \"PAIS\", \"ACTIVO\", \"ID_TIPOS_DOCUMENTO\", \"ID_GENEROS\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\"";

        protected override object ToDb(PERSONAS entity) => new
        {
            entity.NRO_DOCUMENTO,
            entity.NOMBRE,
            entity.APELLIDO,
            entity.FECHA_NACIMIENTO,
            entity.FOTO,
            entity.EMAIL_PRINCIPAL,
            entity.TELEFONO_PRINCIPAL,
            entity.CELULAR_PRINCIPAL,
            entity.CALLE,
            entity.NUMERO,
            entity.PISO,
            entity.DEPARTAMENTO,
            entity.LOCALIDAD,
            entity.PROVINCIA,
            entity.CODIGO_POSTAL,
            entity.PAIS,
            entity.ACTIVO,
            entity.ID_TIPOS_DOCUMENTO,
            entity.ID_GENEROS,
            entity.FECHA_CREACION,
            entity.ULTIMA_ACTUALIZACION
        };

        protected override string InsertSql =>
            $"insert into {Table} (\"NRO_DOCUMENTO\", \"NOMBRE\", \"APELLIDO\", \"FECHA_NACIMIENTO\", \"FOTO\", \"EMAIL_PRINCIPAL\", \"TELEFONO_PRINCIPAL\", \"CELULAR_PRINCIPAL\", \"CALLE\", \"NUMERO\", \"PISO\", \"DEPARTAMENTO\", \"LOCALIDAD\", \"PROVINCIA\", \"CODIGO_POSTAL\", \"PAIS\", \"ACTIVO\", \"ID_TIPOS_DOCUMENTO\", \"ID_GENEROS\", \"FECHA_CREACION\", \"ULTIMA_ACTUALIZACION\") " +
            "values (@NRO_DOCUMENTO, @NOMBRE, @APELLIDO, @FECHA_NACIMIENTO, @FOTO, @EMAIL_PRINCIPAL, @TELEFONO_PRINCIPAL, @CELULAR_PRINCIPAL, @CALLE, @NUMERO, @PISO, @DEPARTAMENTO, @LOCALIDAD, @PROVINCIA, @CODIGO_POSTAL, @PAIS, @ACTIVO, @ID_TIPOS_DOCUMENTO, @ID_GENEROS, @FECHA_CREACION, @ULTIMA_ACTUALIZACION) " +
            $"returning {SelectColumns}";

        protected override string UpdateSql =>
            $"update {Table} set \"NRO_DOCUMENTO\"=@NRO_DOCUMENTO, \"NOMBRE\"=@NOMBRE, \"APELLIDO\"=@APELLIDO, \"FECHA_NACIMIENTO\"=@FECHA_NACIMIENTO, \"FOTO\"=@FOTO, \"EMAIL_PRINCIPAL\"=@EMAIL_PRINCIPAL, \"TELEFONO_PRINCIPAL\"=@TELEFONO_PRINCIPAL, \"CELULAR_PRINCIPAL\"=@CELULAR_PRINCIPAL, \"CALLE\"=@CALLE, \"NUMERO\"=@NUMERO, \"PISO\"=@PISO, \"DEPARTAMENTO\"=@DEPARTAMENTO, \"LOCALIDAD\"=@LOCALIDAD, \"PROVINCIA\"=@PROVINCIA, \"CODIGO_POSTAL\"=@CODIGO_POSTAL, \"PAIS\"=@PAIS, \"ACTIVO\"=@ACTIVO, \"ID_TIPOS_DOCUMENTO\"=@ID_TIPOS_DOCUMENTO, \"ID_GENEROS\"=@ID_GENEROS, \"ULTIMA_ACTUALIZACION\"=@ULTIMA_ACTUALIZACION where \"ID_PERSONAS\"=@ID_PERSONAS";

        public async Task<PERSONAS> InsertWithConnectionAsync(PERSONAS entity, IDbConnection con, IDbTransaction tx, CancellationToken cancellationToken = default)
        {
            return await con.QuerySingleAsync<PERSONAS>(InsertSql, ToDb(entity), tx);
        }

        public async Task<int> GetTipoDocumentoIdByCodigoAsync(string codigo, IDbConnection con, IDbTransaction tx, CancellationToken cancellationToken = default)
        {
            const string sql = "select \"ID_TIPOS_DOCUMENTO\" from public.\"TIPOS_DOCUMENTO\" where \"CODIGO\" = @codigo limit 1";
            return await con.ExecuteScalarAsync<int>(sql, new { codigo }, tx);
        }

        public async Task<int> GetGeneroIdByNombreAsync(string nombre, IDbConnection con, IDbTransaction tx, CancellationToken cancellationToken = default)
        {
            const string sql = "select \"ID_GENEROS\" from public.\"GENEROS\" where \"NOMBRE\" = @nombre limit 1";
            return await con.ExecuteScalarAsync<int>(sql, new { nombre }, tx);
        }
    }
}
