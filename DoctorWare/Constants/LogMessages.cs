
namespace DoctorWare.Constants
{
    public static class LogMessages
    {
        // Information
        public const string DB_CONNECTION_FACTORY_INITIALIZED = "DbConnectionFactory inicializado correctamente";
        public const string POSTGRES_CONNECTION_SUCCESS = "Conexión a PostgreSQL exitosa";
        public const string DATABASE_VERIFICATION_STARTED = "Iniciando verificación de base de datos...";
        public const string DATABASE_INFO = "Base de datos: {DbName}, Usuario: {User}";
        public const string TABLES_FOUND = "Tablas encontradas: {TableCount}";
        public const string TABLE_NAME = "  - {TableName}";
        public const string DATABASE_INITIALIZATION_COMPLETED = "Inicialización de base de datos completada";

        // Information - Startup
        public const string CHECKING_DATABASE_CONNECTION = "Verificando conexión a base de datos ({Ambiente})...";
        public const string STARTING_APPLICATION = "Iniciando aplicación en ambiente: {Ambiente}";
        public const string OPENAPI_DOCUMENTATION_AVAILABLE = "Documentación OpenAPI disponible en: /openapi/v1.json";
        public const string CORS_CONFIGURED_FOR = "CORS configurado para: {Origenes}";


        // Debug
        public const string NEW_POSTGRES_CONNECTION_CREATED = "Nueva conexión a PostgreSQL creada";

        // Error
        public const string POSTGRES_CONNECTION_ERROR = "Error al conectar con PostgreSQL";
        public const string CREATE_POSTGRES_CONNECTION_ERROR = "Error al crear conexión a PostgreSQL";

        // Fatal
        public const string CRITICAL_DATABASE_CONNECTION_ERROR = "Error crítico al conectar con la base de datos";
        public const string APPLICATION_STARTUP_ERROR = "La aplicación falló al iniciar";       

        //Scripts SQL
        public const string STARTING_SQL_SCRIPTS = "Iniciando ejecución de scripts SQL";
        public const string SQL_SCRIPTS_FOUND = "Se encontraron {Cantidad} script(s) SQL";
        public const string EXECUTING_SCRIPT = "Ejecutando: {Archivo}";
        public const string SCRIPT_EXECUTED_SUCCESS = "{Archivo} ejecutado correctamente";
        public const string SCRIPT_ALREADY_EXECUTED = "{Archivo} ya fue ejecutado anteriormente";
        public const string SCRIPT_EXECUTION_ERROR = "Error al ejecutar {Archivo}";
        public const string SQL_SCRIPTS_COMPLETED = "Ejecución de scripts SQL completada";
        public const string SCRIPTS_FOLDER_NOT_FOUND = "La carpeta Scripts no existe en: {Ruta}";
        public const string NO_SQL_SCRIPTS_FOUND = "No se encontraron archivos SQL en la carpeta Scripts";

    }
}
