using Dapper;
using DoctorWare.Constants;
using DoctorWare.Data.Interfaces;
using System.Diagnostics;

namespace DoctorWare.Data
{
    /// <summary>
    /// Ejecuta todos los scripts SQL en orden alfabético cada vez que inicia la aplicación
    /// </summary>
    public class EjecutorScriptsSQL
    {
        private readonly IDbConnectionFactory conexionDB;
        private readonly ILogger<EjecutorScriptsSQL> logger;
        private readonly string carpetaScripts;

        public EjecutorScriptsSQL(
            IDbConnectionFactory conexionDB,
            ILogger<EjecutorScriptsSQL> logger,
            IWebHostEnvironment environment)
        {
            this.conexionDB = conexionDB;
            this.logger = logger;
            carpetaScripts = Path.Combine(environment.ContentRootPath, "Scripts");
        }

        /// <summary>
        /// Ejecuta todos los scripts SQL en orden alfabético
        /// </summary>
        public async Task EjecutarScriptsAsync()
        {
            try
            {
                logger.LogInformation(LogMessages.STARTING_SQL_SCRIPTS);

                if (!Directory.Exists(carpetaScripts))
                {
                    logger.LogWarning(LogMessages.SCRIPTS_FOLDER_NOT_FOUND, carpetaScripts);

                    return;
                }

                List<string> archivosSQL = Directory.GetFiles(carpetaScripts, "*.sql").OrderBy(f => Path.GetFileName(f)).ToList();

                if (!archivosSQL.Any())
                {
                    logger.LogWarning(LogMessages.NO_SQL_SCRIPTS_FOUND);
                    return;
                }

                logger.LogInformation(LogMessages.SQL_SCRIPTS_FOUND, archivosSQL.Count);

                using System.Data.IDbConnection conexion = conexionDB.CreateConnection();
                conexion.Open();

                // Tabla de control para registrar scripts ejecutados (idempotencia)
                const string createMigrationsTable = @"CREATE TABLE IF NOT EXISTS schema_migrations (
                    id SERIAL PRIMARY KEY,
                    script_name TEXT NOT NULL UNIQUE,
                    executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                );";
                await conexion.ExecuteAsync(createMigrationsTable);

                int scriptEjecutados = 0;
                int scriptConErrores = 0;

                foreach (string archivoSQL in archivosSQL)
                {
                    string nombreArchivo = Path.GetFileName(archivoSQL);

                    try
                    {
                        logger.LogInformation(LogMessages.EXECUTING_SCRIPT, nombreArchivo);

                        string scriptSQL = await File.ReadAllTextAsync(archivoSQL);

                        if (string.IsNullOrWhiteSpace(scriptSQL))
                        {
                            logger.LogWarning("El archivo {Archivo} está vacío", nombreArchivo);
                            continue;
                        }

                        // Si ya fue ejecutado, saltar
                        int yaEjecutado = await conexion.ExecuteScalarAsync<int>(
                            "SELECT COUNT(1) FROM schema_migrations WHERE script_name = @name",
                            new { name = nombreArchivo });
                        if (yaEjecutado > 0)
                        {
                            logger.LogInformation(LogMessages.SCRIPT_ALREADY_EXECUTED, nombreArchivo);
                            continue;
                        }

                        // Ejecutar script en transacción y registrar
                        using var tx = conexion.BeginTransaction();
                        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        await conexion.ExecuteAsync(scriptSQL, commandTimeout: 300, transaction: tx);
                        await conexion.ExecuteAsync(
                            "INSERT INTO schema_migrations (script_name) VALUES (@name)",
                            new { name = nombreArchivo }, tx);
                        // IDbTransaction no expone CommitAsync; usar Commit sincrónico
                        tx.Commit();
                        stopwatch.Stop();

                        scriptEjecutados++;
                        logger.LogInformation(LogMessages.SCRIPT_EXECUTED_SUCCESS + " ({Tiempo}ms)", nombreArchivo, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        scriptConErrores++;
                        logger.LogError(ex, LogMessages.SCRIPT_EXECUTION_ERROR + ": {Mensaje}",
                            nombreArchivo, ex.Message);

                        throw; 
                    }
                }

                logger.LogInformation(
                    LogMessages.SQL_SCRIPTS_COMPLETED + " - Ejecutados: {Ejecutados}, Errores: {Errores}",
                    scriptEjecutados, scriptConErrores);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error crítico al ejecutar scripts SQL");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta un script SQL específico por nombre
        /// </summary>
        public async Task EjecutarScriptEspecificoAsync(string nombreArchivo)
        {
            string rutaCompleta = Path.Combine(carpetaScripts, nombreArchivo);

            if (!File.Exists(rutaCompleta))
            {
                throw new FileNotFoundException($"El archivo {nombreArchivo} no existe en la carpeta Scripts", rutaCompleta);
            }

            logger.LogInformation("Ejecutando script específico: {Archivo}", nombreArchivo);

            string scriptSQL = await File.ReadAllTextAsync(rutaCompleta);

            using System.Data.IDbConnection conexion = conexionDB.CreateConnection();
            await conexion.ExecuteAsync(scriptSQL, commandTimeout: 300);

            logger.LogInformation("Script {Archivo} ejecutado correctamente", nombreArchivo);
        }
    }
}
