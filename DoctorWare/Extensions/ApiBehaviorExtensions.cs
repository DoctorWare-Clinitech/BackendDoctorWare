using DoctorWare.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Extensions
{
    /// <summary>
    /// Extensiones para configurar comportamiento API estandarizado (model validation, respuestas homogéneas).
    /// </summary>
    public static class ApiBehaviorExtensions
    {
        /// <summary>
        /// Configura una respuesta estándar para errores de validación de modelo (400) usando ApiResponse.
        /// </summary>
        public static IServiceCollection AddStandardizedApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    object errors = context.ModelState
                        .Where(e => e.Value is not null && e.Value.Errors.Count > 0)
                        .Select(kvp => new
                        {
                            Field = kvp.Key,
                            Errors = kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage).ToArray()
                        })
                        .ToArray();

                    ApiResponse<object> response = new ApiResponse<object>(false, errors, "La solicitud es inválida.", "validation_error");
                    return new BadRequestObjectResult(response);
                };
            });

            return services;
        }
    }
}
