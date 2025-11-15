using DoctorWare.DTOs.Requests.Patients;
using DoctorWare.DTOs.Response.Patients;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientsService patientsService;
        private readonly IMedicalHistoryService medicalHistoryService;

        public PatientsController(IPatientsService patientsService, IMedicalHistoryService medicalHistoryService)
        {
            this.patientsService = patientsService;
            this.medicalHistoryService = medicalHistoryService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PatientDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] string? name,
            [FromQuery] string? dni,
            [FromQuery] string? email,
            [FromQuery] string? phone,
            [FromQuery] string? professionalId,
            [FromQuery] bool? isActive,
            CancellationToken ct)
        {
            IEnumerable<PatientDto> list = await patientsService.GetAsync(name, dni, email, phone, professionalId, isActive, ct);
            return Ok(list);
        }

        [HttpGet("summary")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PatientSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary([FromQuery] string? professionalId, CancellationToken ct)
        {
            IEnumerable<PatientSummaryDto> list = await patientsService.GetSummaryAsync(professionalId, ct);
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct)
        {
            PatientDto? dto = await patientsService.GetByIdAsync(id, ct);
            if (dto is null)
            {
                return NotFound(new { message = "Paciente no encontrado" });
            }
            return Ok(dto);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
        {
            PatientDto created = await patientsService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
        {
            PatientDto updated = await patientsService.UpdateAsync(id, request, ct);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken ct)
        {
            await patientsService.DeleteAsync(id, ct);
            return NoContent();
        }

        // GET /api/patients/{id}/history -> alias conveniente para el front
        [HttpGet("{id}/history")]
        [Authorize]
        public async Task<IActionResult> History([FromRoute] string id, CancellationToken ct)
        {
            IEnumerable<DoctorWare.DTOs.Response.Medical.MedicalHistoryDto> list = await medicalHistoryService.GetByPatientAsync(id, ct);
            return Ok(list);
        }
    }
}
