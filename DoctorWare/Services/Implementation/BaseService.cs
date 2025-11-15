using DoctorWare.DTOs.Response;
using DoctorWare.Repositories.Interfaces;

namespace DoctorWare.Services.Implementation
{
    /// <summary>
    /// Servicio base genérico que implementa operaciones CRUD comunes apoyándose en un repositorio.
    /// Expone hooks virtuales para permitir overrides de validación y de lógica antes/después de guardar.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad de dominio.</typeparam>
    /// <typeparam name="TKey">Tipo de la clave primaria.</typeparam>
    public abstract class BaseService<T, TKey> : Services.Interfaces.IService<T, TKey>
    {
        /// <summary>
        /// Repositorio subyacente utilizado por el servicio.
        /// </summary>
        protected readonly IRepository<T, TKey> Repository;

        /// <summary>
        /// Crea una instancia del servicio base con el repositorio requerido (DIP).
        /// </summary>
        protected BaseService(IRepository<T, TKey> repository)
        {
            Repository = repository;
        }

        /// <summary>
        /// Hook de validación para overridear reglas de negocio antes de crear/actualizar.
        /// </summary>
        protected virtual Task ValidateAsync(T entity) => Task.CompletedTask;

        /// <summary>
        /// Hook previo al guardado (crear/actualizar) para lógica adicional.
        /// </summary>
        protected virtual Task BeforeSaveAsync(T entity) => Task.CompletedTask;

        /// <summary>
        /// Hook posterior al guardado (crear/actualizar) para lógica adicional.
        /// </summary>
        protected virtual Task AfterSaveAsync(T entity) => Task.CompletedTask;

        /// <inheritdoc />
        public virtual Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
            => Repository.GetByIdAsync(id, cancellationToken);

        /// <inheritdoc />
        public virtual Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
            => Repository.GetPagedAsync(page, pageSize, cancellationToken);

        /// <inheritdoc />
        public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(entity);
            await BeforeSaveAsync(entity);
            T created = await Repository.InsertAsync(entity, cancellationToken);
            await AfterSaveAsync(created);
            return created;
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(entity);
            await BeforeSaveAsync(entity);
            await Repository.UpdateAsync(entity, cancellationToken);
            await AfterSaveAsync(entity);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
            => Repository.DeleteAsync(id, cancellationToken);
    }
}
