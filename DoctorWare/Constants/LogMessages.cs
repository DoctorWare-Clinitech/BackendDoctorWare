
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
    }
}
