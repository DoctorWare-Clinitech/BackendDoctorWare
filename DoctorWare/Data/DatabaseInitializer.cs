using Dapper;
using DoctorWare.Constants;
using DoctorWare.Data.Interfaces;
using DoctorWare.Data.Models;
using DoctorWare.Exceptions;
using System.Data;

namespace DoctorWare.Data
{
    /// <summary>
    /// Inicializador de base de datos - Verifica conexión y puede crear tablas si no existen
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly ILogger<DatabaseInitializer> logger;

        public DatabaseInitializer(
            IDbConnectionFactory connectionFactory,
            ILogger<DatabaseInitializer> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Verifica que la conexión a la base de datos funcione correctamente
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using IDbConnection connection = connectionFactory.CreateConnection();
                int result = await connection.ExecuteScalarAsync<int>("SELECT 1");

                logger.LogInformation(LogMessages.POSTGRES_CONNECTION_SUCCESS);
                return result == 1;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, LogMessages.POSTGRES_CONNECTION_ERROR);
                return false;
            }
        }

        /// <summary>
        /// Obtiene información de la base de datos
        /// </summary>
        public async Task<DataBaseInfo> GetDatabaseInfoAsync()
        {
            using IDbConnection connection = connectionFactory.CreateConnection();

            string version = await connection.ExecuteScalarAsync<string>("SELECT version()");
            string dbName = await connection.ExecuteScalarAsync<string>("SELECT current_database()");
            string user = await connection.ExecuteScalarAsync<string>("SELECT current_user");

            return new DataBaseInfo
            {
                Version = version ?? ErrorMessages.UNKNOWN,
                DatabaseName = dbName ?? ErrorMessages.UNKNOWN,
                User = user ?? ErrorMessages.UNKNOWN
            };
        }

        /// <summary>
        /// Verifica si una tabla existe
        /// </summary>
        public async Task<bool> TableExistsAsync(string tableName)
        {
            using IDbConnection connection = connectionFactory.CreateConnection();

            string sql = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = @TableName
                )";

            return await connection.ExecuteScalarAsync<bool>(sql, new { TableName = tableName.ToLower() });
        }

        /// <summary>
        /// Lista todas las tablas de la base de datos
        /// </summary>
        public async Task<IEnumerable<string>> GetAllTablesAsync()
        {
            using IDbConnection connection = connectionFactory.CreateConnection();

            string sql = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                ORDER BY table_name";

            return await connection.QueryAsync<string>(sql);
        }

        /// <summary>
        /// Ejecuta scripts SQL de inicialización (opcional - para crear tablas si no existen)
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            bool isConnected = await TestConnectionAsync();
            DataBaseInfo dbInfo = await GetDatabaseInfoAsync();
            IEnumerable<string> tables = await GetAllTablesAsync();

            logger.LogInformation(LogMessages.DATABASE_VERIFICATION_STARTED);

            if (!isConnected)
            {
                throw new DatabaseConnectionException(ErrorMessages.DB_CANNOT_CONNECT);
            }

            logger.LogInformation(LogMessages.DATABASE_INFO, dbInfo.DatabaseName, dbInfo.User);
            logger.LogInformation(LogMessages.TABLES_FOUND, tables.Count());

            foreach (string table in tables)
            {
                logger.LogInformation(LogMessages.TABLE_NAME, table);
            }

            logger.LogInformation(LogMessages.DATABASE_INITIALIZATION_COMPLETED);
        }
    }
}
