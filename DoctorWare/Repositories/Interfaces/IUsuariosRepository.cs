using DoctorWare.Models;

namespace DoctorWare.Repositories.Interfaces
{
    public interface IUsuariosRepository : IRepository<USUARIOS, int>
    {
        Task<USUARIOS?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<USUARIOS> InsertWithConnectionAsync(USUARIOS entity, System.Data.IDbConnection con, System.Data.IDbTransaction tx, CancellationToken cancellationToken = default);
    }
}

