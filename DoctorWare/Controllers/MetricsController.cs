using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    [Produces("application/json")]
    public class MetricsController : ControllerBase
    {
        private readonly IRequestMetricsService metricsService;

        public MetricsController(IRequestMetricsService metricsService)
        {
            this.metricsService = metricsService;
        }

        /// <summary>
        /// Resumen de métricas básicas de desempeño de la API.
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Roles = "admin,professional")]
        public IActionResult Summary()
        {
            RequestMetricsSnapshot snapshot = metricsService.GetSnapshot();
            return Ok(snapshot);
        }
    }
}
