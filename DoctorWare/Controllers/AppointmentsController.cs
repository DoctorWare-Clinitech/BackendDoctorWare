using DoctorWare.DTOs.Requests.Appointments;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.Helpers;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService service;

        public AppointmentsController(IAppointmentsService service)
        {
            this.service = service;
        }

        // GET /api/appointments
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] string? professionalId,
            [FromQuery] string? patientId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status,
            [FromQuery] string? type,
            CancellationToken ct)
        {
            IEnumerable<AppointmentDto> list = await service.GetAsync(professionalId, patientId, startDate, endDate, status, type, ct);
            return Ok(list);
        }

        // GET /api/appointments/{id}
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct)
        {
            AppointmentDto? dto = await service.GetByIdAsync(id, ct);
            if (dto is null)
            {
                return NotFound(new { message = "Turno no encontrado" });
            }
            return Ok(dto);
        }

        // POST /api/appointments
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
        {
            int? createdBy = User.GetUserId();
            AppointmentDto created = await service.CreateAsync(request, createdBy, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/appointments/{id}
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct)
        {
            int? updatedBy = User.GetUserId();
            AppointmentDto updated = await service.UpdateAsync(id, request, updatedBy, ct);
            return Ok(updated);
        }

        // DELETE /api/appointments/{id}
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromRoute] string id, [FromQuery] string? reason, CancellationToken ct)
        {
            int? userId = User.GetUserId();
            await service.CancelAsync(id, userId, reason, ct);
            return NoContent();
        }

        // GET /api/appointments/stats
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> Stats([FromQuery] string? professionalId, CancellationToken ct)
        {
            (int total, int scheduled, int confirmed, int completed, int cancelled, int noShow) = await service.GetStatsAsync(professionalId, ct);
            return Ok(new { total, scheduled, confirmed, completed, cancelled, noShow });
        }
    }
}
