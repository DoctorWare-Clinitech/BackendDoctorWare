using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;
using DoctorWare.Helpers;
using DoctorWare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationsService service;

        public MedicationsController(IMedicationsService service)
        {
            this.service = service;
        }

        [HttpGet("patient/{patientId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<MedicationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPatient([FromRoute] string patientId, CancellationToken ct)
        {
            IEnumerable<MedicationDto> list = await service.GetByPatientAsync(patientId, ct);
            return Ok(list);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateMedicationRequest request, CancellationToken ct)
        {
            int? createdBy = User.GetUserId();
            MedicationDto created = await service.CreateAsync(request, createdBy, ct);
            return CreatedAtAction(nameof(GetByPatient), new { patientId = request.PatientId }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateMedicationRequest request, CancellationToken ct)
        {
            MedicationDto updated = await service.UpdateAsync(id, request, ct);
            return Ok(updated);
        }

        [HttpPatch("{id}/discontinue")]
        [Authorize]
        [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Discontinue([FromRoute] string id, CancellationToken ct)
        {
            MedicationDto updated = await service.DiscontinueAsync(id, ct);
            return Ok(updated);
        }
    }
}

