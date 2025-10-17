namespace DoctorWare.DTOs.Response
{
    /// <summary>
    /// Contenedor estandarizado para respuestas de la API.
    /// Incluye estado de éxito, datos, mensaje y un código de error opcional.
    /// </summary>
    /// <typeparam name="T">Tipo del dato que retorna la operación.</typeparam>
    public record ApiResponse<T>(bool Success, T? Data, string? Message = null, string? ErrorCode = null)
    {
        /// <summary>
        /// Crea una respuesta exitosa (200 OK) con datos.
        /// </summary>
        public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message);

        /// <summary>
        /// Crea una respuesta de recurso creado (201 Created) con datos.
        /// </summary>
        public static ApiResponse<T> Created(T data, string? message = null) => new(true, data, message);

        /// <summary>
        /// Crea una respuesta de error con mensaje y código opcional.
        /// </summary>
        public static ApiResponse<T> Fail(string message, string? errorCode = null) => new(false, default, message, errorCode);
    }
}

