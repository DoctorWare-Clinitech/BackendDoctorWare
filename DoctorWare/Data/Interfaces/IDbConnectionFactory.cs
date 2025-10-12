using System.Data;

namespace DoctorWare.Data.Interfaces
{
    /// <summary>
    /// Factory para crear conexiones a la base de datos
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Crea una nueva conexión a la base de datos
        /// </summary>
        IDbConnection CreateConnection();

        /// <summary>
        /// Obtiene el connection string configurado
        /// </summary>
        string GetConnectionString();
    }
}
