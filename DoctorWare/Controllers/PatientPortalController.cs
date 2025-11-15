using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.DTOs.Response.Medical;
using DoctorWare.Helpers;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Produces("application/json")]
    [Authorize(Roles = "patient")]
    public class PatientPortalController : ControllerBase
    {
        private readonly IPatientPortalService patientPortalService;

        public PatientPortalController(IPatientPortalService patientPortalService)
        {
            this.patientPortalService = patientPortalService;
        }

        /// <summary>
        /// Lista los turnos del paciente autenticado.
        /// </summary>
        [HttpGet("appointments")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppointments([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken ct)
        {
            int? userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            IEnumerable<AppointmentDto> list = await patientPortalService.GetAppointmentsAsync(userId.Value, startDate, endDate, ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene un turno del paciente autenticado.
        /// </summary>
        [HttpGet("appointments/{appointmentId}")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAppointment([FromRoute] string appointmentId, CancellationToken ct)
        {
            int? userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            AppointmentDto? dto = await patientPortalService.GetAppointmentAsync(userId.Value, appointmentId, ct);
            if (dto is null)
            {
                return NotFound(new { message = "Turno no encontrado" });
            }

            return Ok(dto);
        }

        /// <summary>
        /// Cancela un turno del paciente autenticado.
        /// </summary>
        [HttpDelete("appointments/{appointmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CancelAppointment([FromRoute] string appointmentId, [FromQuery] string? reason, CancellationToken ct)
        {
            int? userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            bool ok = await patientPortalService.CancelAppointmentAsync(userId.Value, appointmentId, reason, ct);
            if (!ok)
            {
                return NotFound(new { message = "Turno no encontrado" });
            }

            return NoContent();
        }

        /// <summary>
        /// Historia clínica del paciente autenticado.
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(IEnumerable<MedicalHistoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory(CancellationToken ct)
        {
            int? userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            IEnumerable<MedicalHistoryDto> history = await patientPortalService.GetHistoryAsync(userId.Value, ct);
            return Ok(history);
        }
    }
}
