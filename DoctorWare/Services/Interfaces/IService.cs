using DoctorWare.DTOs.Response;

namespace DoctorWare.Services.Interfaces
{
    /// <summary>
    /// Contrato genérico para operaciones de negocio (Service) sobre una entidad.
    /// Define un conjunto común de acciones CRUD y paginación.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad de dominio.</typeparam>
    /// <typeparam name="TKey">Tipo de la clave primaria de la entidad.</typeparam>
    public interface IService<T, TKey>
    {
        /// <summary>Obtiene una entidad por su identificador.</summary>
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>Obtiene un listado paginado de entidades.</summary>
        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>Crea una nueva entidad y retorna su versión creada.</summary>
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Actualiza una entidad existente.</summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Elimina una entidad por su identificador.</summary>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
