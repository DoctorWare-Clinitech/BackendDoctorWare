using DoctorWare.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    /// <summary>
    /// Controlador base para la API.
    /// Proporciona helpers estandarizados para respuestas homogéneas y facilita herencia/override.
    /// Heredar de esta clase evita duplicar código repetitivo de respuesta.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Retorna una respuesta 200 OK con formato estándar.
        /// </summary>
        protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
            => Ok(ApiResponse<T>.Ok(data, message));

        /// <summary>
        /// Retorna una respuesta 201 Created con formato estándar y encabezado Location.
        /// </summary>
        protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string location, T data, string? message = null)
            => Created(location, ApiResponse<T>.Created(data, message));

        /// <summary>
        /// Retorna una respuesta de error con el código de estado indicado y formato estándar.
        /// </summary>
        protected ActionResult<ApiResponse<object>> ErrorResponse(string message, int status = 400, string? code = null)
            => StatusCode(status, ApiResponse<object>.Fail(message, code));
    }
}

