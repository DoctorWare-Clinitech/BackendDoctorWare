using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;
using DoctorWare.Helpers;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly IMedicalHistoryService service;

        public MedicalHistoryController(IMedicalHistoryService service)
        {
            this.service = service;
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(IEnumerable<MedicalHistoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPatient([FromRoute] string patientId, CancellationToken ct)
        {
            IEnumerable<MedicalHistoryDto> list = await service.GetByPatientAsync(patientId, ct);
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(MedicalHistoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct)
        {
            MedicalHistoryDto? dto = await service.GetByIdAsync(id, ct);
            if (dto is null)
            {
                return NotFound(new { message = "Registro médico no encontrado" });
            }
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(MedicalHistoryDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateMedicalHistoryRequest request, CancellationToken ct)
        {
            int? createdBy = User.GetUserId();
            MedicalHistoryDto created = await service.CreateAsync(request, createdBy, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(MedicalHistoryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateMedicalHistoryRequest request, CancellationToken ct)
        {
            MedicalHistoryDto updated = await service.UpdateAsync(id, request, ct);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "professional,admin")]
        public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken ct)
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
