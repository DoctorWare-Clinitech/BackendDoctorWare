using DoctorWare.Models;

namespace DoctorWare.Repositories.Interfaces
{
    public interface IPersonasRepository : IRepository<PERSONAS, int>
    {
        Task<PERSONAS> InsertWithConnectionAsync(PERSONAS entity, System.Data.IDbConnection con, System.Data.IDbTransaction tx, CancellationToken cancellationToken = default);

        Task<int> GetTipoDocumentoIdByCodigoAsync(string codigo, System.Data.IDbConnection con, System.Data.IDbTransaction tx, CancellationToken cancellationToken = default);

        Task<int> GetGeneroIdByNombreAsync(string nombre, System.Data.IDbConnection con, System.Data.IDbTransaction tx, CancellationToken cancellationToken = default);
    }
}

