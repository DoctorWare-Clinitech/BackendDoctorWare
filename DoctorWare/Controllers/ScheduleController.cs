using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.DTOs.Requests.Schedule;
using DoctorWare.DTOs.Response.Frontend;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }

        /// <summary>
        /// Obtiene la configuración de agenda de un profesional (horarios y duración de consulta).
        /// </summary>
        [HttpGet("{professionalId}")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(ScheduleConfigDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfig([FromRoute] string professionalId, CancellationToken ct)
        {
            ScheduleConfigDto config = await scheduleService.GetConfigAsync(professionalId, ct);
            return Ok(config);
        }

        /// <summary>
        /// Actualiza la configuración general de agenda de un profesional.
        /// </summary>
        [HttpPut("{professionalId}")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(ScheduleConfigDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateConfig([FromRoute] string professionalId, [FromBody] UpdateScheduleConfigRequest request, CancellationToken ct)
        {
            ScheduleConfigDto updated = await scheduleService.UpdateConfigAsync(professionalId, request, ct);
            return Ok(updated);
        }

        /// <summary>
        /// Agrega un horario disponible a la agenda de un profesional.
        /// </summary>
        [HttpPost("{professionalId}/slots")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(ScheduleTimeSlotDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddTimeSlot([FromRoute] string professionalId, [FromBody] CreateTimeSlotRequest request, CancellationToken ct)
        {
            ScheduleTimeSlotDto created = await scheduleService.AddTimeSlotAsync(professionalId, request, ct);
            return CreatedAtAction(nameof(GetConfig), new { professionalId }, created);
        }

        /// <summary>
        /// Actualiza un horario disponible existente.
        /// </summary>
        [HttpPut("{professionalId}/slots/{slotId}")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(ScheduleTimeSlotDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTimeSlot([FromRoute] string professionalId, [FromRoute] string slotId, [FromBody] UpdateTimeSlotRequest request, CancellationToken ct)
        {
            ScheduleTimeSlotDto updated = await scheduleService.UpdateTimeSlotAsync(professionalId, slotId, request, ct);
            return Ok(updated);
        }

        /// <summary>
        /// Elimina un horario disponible.
        /// </summary>
        [HttpDelete("{professionalId}/slots/{slotId}")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTimeSlot([FromRoute] string professionalId, [FromRoute] string slotId, CancellationToken ct)
        {
            await scheduleService.DeleteTimeSlotAsync(professionalId, slotId, ct);
            return NoContent();
        }

        /// <summary>
        /// Crea un bloqueo de agenda (ausencia) para un profesional.
        /// </summary>
        [HttpPost("{professionalId}/blocks")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(ScheduleBlockedSlotDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> BlockSlot([FromRoute] string professionalId, [FromBody] CreateBlockedSlotRequest request, CancellationToken ct)
        {
            ScheduleBlockedSlotDto created = await scheduleService.AddBlockedSlotAsync(professionalId, request, ct);
            return CreatedAtAction(nameof(GetBlockedSlots), new { professionalId }, created);
        }

        /// <summary>
        /// Elimina un bloqueo de agenda.
        /// </summary>
        [HttpDelete("{professionalId}/blocks/{blockId}")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnblockSlot([FromRoute] string professionalId, [FromRoute] string blockId, CancellationToken ct)
        {
            await scheduleService.DeleteBlockedSlotAsync(professionalId, blockId, ct);
            return NoContent();
        }

        /// <summary>
        /// Obtiene los bloques de agenda de un profesional entre dos fechas.
        /// </summary>
        [HttpGet("{professionalId}/blocks")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(IEnumerable<ScheduleBlockedSlotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBlockedSlots(
            [FromRoute] string professionalId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken ct)
        {
            IEnumerable<ScheduleBlockedSlotDto> list = await scheduleService.GetBlockedSlotsAsync(professionalId, startDate, endDate, ct);
            return Ok(list);
        }

        /// <summary>
        /// Obtiene los horarios disponibles para un profesional en una fecha dada.
        /// </summary>
        [HttpGet("{professionalId}/available")]
        [Authorize(Roles = "professional,secretary,admin")]
        [ProducesResponseType(typeof(IEnumerable<ScheduleAvailableSlotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableSlots(
            [FromRoute] string professionalId,
            [FromQuery] DateTime date,
            CancellationToken ct)
        {
            IEnumerable<ScheduleAvailableSlotDto> list = await scheduleService.GetAvailableSlotsAsync(professionalId, date, ct);
            return Ok(list);
        }
    }
}
