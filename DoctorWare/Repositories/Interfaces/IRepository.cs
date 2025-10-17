using DoctorWare.DTOs.Response;

namespace DoctorWare.Repositories.Interfaces
{
    /// <summary>
    /// Contrato genérico para repositorios (capa de acceso a datos) con operaciones CRUD y paginación.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad persistida.</typeparam>
    /// <typeparam name="TKey">Tipo de la clave primaria.</typeparam>
    public interface IRepository<T, TKey>
    {
        /// <summary>Obtiene una entidad por identificador.</summary>
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>Obtiene un listado paginado de entidades.</summary>
        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>Inserta una entidad y retorna la entidad creada (habitualmente con su clave).</summary>
        Task<T> InsertAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Actualiza una entidad existente.</summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Elimina una entidad por identificador.</summary>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
