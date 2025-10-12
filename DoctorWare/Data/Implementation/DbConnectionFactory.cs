using DoctorWare.Constants;
using DoctorWare.Data.Interfaces;
using DoctorWare.Exceptions;
using Npgsql;
using System.Data;

namespace DoctorWare.Data.Implementation
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string cadenaConexion;
        private readonly ILogger<DbConnectionFactory> logger;

        public DbConnectionFactory(
            IConfiguration configuracion,
            ILogger<DbConnectionFactory> logger)
        {
            this.logger = logger;
           
            cadenaConexion = configuracion.GetConnectionString("ConexionPredeterminada")
                ?? throw new ArgumentNullException(nameof(configuracion),
                    "Connection string 'ConexionPredeterminada' no está configurada en appsettings.json");

            this.logger.LogInformation(LogMessages.DB_CONNECTION_FACTORY_INITIALIZED);
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                NpgsqlConnection conexion = new NpgsqlConnection(cadenaConexion);
                logger.LogDebug(LogMessages.NEW_POSTGRES_CONNECTION_CREATED);
                return conexion;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, LogMessages.CREATE_POSTGRES_CONNECTION_ERROR);
                throw new DatabaseConnectionException(ex);
            }
        }

        public string GetConnectionString()
        {
            return cadenaConexion;
        }
    }
}