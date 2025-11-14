namespace DoctorWare.Constants
{
    public static class ErrorMessages
    {
        public const string ERRO_GENERICO = "Ocurrió un error inesperado.";
        public const string NOT_FOUND = "El recurso solicitado no fue encontrado.";
        public const string BAD_REQUEST = "La solicitud es inválida.";
        public const string DB_CONNECTION_ERROR = "Error al crear la conexión con la base de datos.";
        public const string DB_CANNOT_CONNECT = "No se pudo conectar a la base de datos";
        public const string UNKNOWN = "Unknown";

        // Auth / Usuarios
        public const string EMAIL_ALREADY_REGISTERED = "El email ya está registrado.";
        public const string INVALID_CREDENTIALS = "Credenciales inválidas.";
        public const string INVALID_REFRESH_TOKEN = "Refresh token inválido o expirado.";
        public const string EMAIL_NOT_CONFIRMED = "El email no está confirmado.";

        // Registro Profesional / Paciente
        public const string CUIT_INVALID = "CUIT/CUIL inválido.";
        public const string CUIT_DUPLICADO = "El CUIT/CUIL ya está registrado.";
        public const string MATRICULA_NACIONAL_DUPLICADA = "La matrícula nacional ya está registrada.";
        public const string MATRICULA_PROVINCIAL_DUPLICADA = "La matrícula provincial ya está registrada.";
        public const string ESPECIALIDAD_NO_ENCONTRADA = "La especialidad indicada no existe.";
    }
}
