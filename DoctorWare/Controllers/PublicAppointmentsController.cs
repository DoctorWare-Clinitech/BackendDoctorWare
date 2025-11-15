using System.Threading;
using System.Threading.Tasks;
using DoctorWare.DTOs.Requests.Public;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/public/appointments")]
    [Produces("application/json")]
    public class PublicAppointmentsController : ControllerBase
    {
        private readonly IPublicAppointmentsService publicAppointmentsService;

        public PublicAppointmentsController(IPublicAppointmentsService publicAppointmentsService)
        {
            this.publicAppointmentsService = publicAppointmentsService;
        }

        /// <summary>
        /// Crea un turno a nombre del paciente desde el portal público.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> RequestAppointment([FromBody] PublicAppointmentRequest request, CancellationToken ct)
        {
            AppointmentDto created = await publicAppointmentsService.RequestAppointmentAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, created);
        }
    }
}
