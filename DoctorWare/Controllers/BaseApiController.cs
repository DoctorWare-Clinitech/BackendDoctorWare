using DoctorWare.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    /// <summary>
    /// Controlador base con utilidades para respuestas estandarizadas.
    /// Las clases derivadas deben decorarse con [ApiController] y [Route].
    /// </summary>
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
        {
            return Ok(ApiResponse<T>.Ok(data, message));
        }

        protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string location, T data, string? message = null)
        {
            return Created(location, ApiResponse<T>.Created(data, message));
        }

        protected ActionResult<ApiResponse<object>> ErrorResponse(string message, int status = 400, string? code = null)
        {
            return StatusCode(status, ApiResponse<object>.Fail(message, code));
        }

        protected ActionResult<ApiResponse<T>> ErrorResponse<T>(string message, int status = 400, string? code = null)
        {
            return StatusCode(status, ApiResponse<T>.Fail(message, code));
        }
    }
}
