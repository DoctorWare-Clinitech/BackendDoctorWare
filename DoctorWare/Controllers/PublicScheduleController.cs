using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.DTOs.Response.Frontend;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/public/professionals")]
    [Produces("application/json")]
    public class PublicScheduleController : ControllerBase
    {
        private readonly IScheduleService scheduleService;

        public PublicScheduleController(IScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }

        /// <summary>
        /// Obtiene la disponibilidad pública de un profesional para una fecha determinada.
        /// </summary>
        [HttpGet("{professionalId}/availability")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ScheduleAvailableSlotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailability([FromRoute] string professionalId, [FromQuery] DateTime? date, CancellationToken ct)
        {
            DateTime targetDate = date?.Date ?? DateTime.UtcNow.Date;
            IEnumerable<ScheduleAvailableSlotDto> slots = await scheduleService.GetAvailableSlotsAsync(professionalId, targetDate, ct);
            return Ok(slots);
        }
    }
}
