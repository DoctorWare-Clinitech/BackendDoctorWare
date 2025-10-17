using DoctorWare.DTOs.Response;
using DoctorWare.Exceptions;

namespace DoctorWare.Middleware
{
    /// <summary>
    /// Middleware global para captura y estandarización de excepciones.
    /// Convierte excepciones en respuestas JSON homogéneas (ApiResponse) y registra el error.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlingMiddleware> logger;
        private readonly IHostEnvironment env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            this.next = next;
            this.logger = logger;
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            int status;
            string code;
            string message;

            switch (ex)
            {
                case NotFoundException:
                    status = StatusCodes.Status404NotFound;
                    code = "not_found";
                    message = ex.Message;
                    break;
                case BadRequestException:
                    status = StatusCodes.Status400BadRequest;
                    code = "bad_request";
                    message = ex.Message;
                    break;
                case DatabaseConnectionException:
                    status = StatusCodes.Status500InternalServerError;
                    code = "db_connection_error";
                    message = ex.Message;
                    break;
                case UnauthorizedAccessException:
                    status = StatusCodes.Status401Unauthorized;
                    code = "unauthorized";
                    message = ex.Message;
                    break;
                case KeyNotFoundException:
                    status = StatusCodes.Status404NotFound;
                    code = "not_found";
                    message = ex.Message;
                    break;
                default:
                    status = StatusCodes.Status500InternalServerError;
                    code = "server_error";
                    message = env.IsDevelopment() ? ex.Message : "Ha ocurrido un error inesperado.";
                    break;
            }

            // Log detallado
            logger.LogError(ex, "[{Code}] {Message}", code, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = status;

            var payload = new ApiResponse<object>(false,
                env.IsDevelopment() ? new { detail = ex.Message, stackTrace = ex.StackTrace } : null,
                message,
                code);

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
}

