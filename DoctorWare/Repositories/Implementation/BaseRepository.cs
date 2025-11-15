using Dapper;
using DoctorWare.Data.Interfaces;
using System.Data;

namespace DoctorWare.Repositories.Implementation
{
    /// <summary>
    /// Repositorio base genérico con operaciones CRUD usando Dapper.
    /// Implementa el patrón Template Method: las subclases definen tabla, clave y SQL específico.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad persistida.</typeparam>
    /// <typeparam name="TKey">Tipo de la clave primaria.</typeparam>
    public abstract class BaseRepository<T, TKey> : Interfaces.IRepository<T, TKey>
    {
        private readonly IDbConnectionFactory _factory;

        /// <summary>
        /// Crea una instancia del repositorio base con una fábrica de conexiones (DIP).
        /// </summary>
        protected BaseRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Nombre de la tabla subyacente. Debe ser definido por la subclase.
        /// </summary>
        protected abstract string Table { get; }

        /// <summary>
        /// Nombre de la columna clave primaria. Por defecto 'id'.
        /// </summary>
        protected virtual string Key => "id";

        /// <summary>
        /// Columnas a seleccionar. Por defecto '*'.
        /// </summary>
        protected virtual string SelectColumns => "*";

        /// <summary>
        /// Mapeo de entidad a parámetros para Dapper.
        /// </summary>
        protected abstract object ToDb(T entity);

        /// <summary>
        /// SQL de inserción que debe retornar la fila creada (RETURNING ...).
        /// </summary>
        protected abstract string InsertSql { get; }

        /// <summary>
        /// SQL de actualización (WHERE {Key} = @Id).
        /// </summary>
        protected abstract string UpdateSql { get; }

        /// <summary>
        /// Crea una conexión nueva usando la fábrica inyectada.
        /// </summary>
        protected IDbConnection CreateConnection() => _factory.CreateConnection();

        /// <inheritdoc />
        public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            string sql = $"select {SelectColumns} from {Table} where {Key} = @id";
            return await con.QuerySingleOrDefaultAsync<T>(sql, new { id });
        }

        /// <inheritdoc />
        public virtual async Task<DoctorWare.DTOs.Response.PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            int offset = (page - 1) * pageSize;
            IEnumerable<T> itemsEnumerable = await con.QueryAsync<T>($"select {SelectColumns} from {Table} order by {Key} offset @offset limit @limit",
                new { offset, limit = pageSize });
            int total = await con.ExecuteScalarAsync<int>($"select count(*) from {Table}");
            List<T> items = itemsEnumerable.ToList();
            return new DoctorWare.DTOs.Response.PagedResult<T>(items, total, page, pageSize);
        }

        /// <inheritdoc />
        public virtual async Task<T> InsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            return await con.QuerySingleAsync<T>(InsertSql, ToDb(entity));
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            await con.ExecuteAsync(UpdateSql, ToDb(entity));
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            using IDbConnection con = CreateConnection();
            await con.ExecuteAsync($"delete from {Table} where {Key} = @id", new { id });
        }
    }
}
