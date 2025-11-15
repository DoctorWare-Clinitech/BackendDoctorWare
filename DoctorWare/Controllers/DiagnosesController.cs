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
    public class DiagnosesController : ControllerBase
    {
        private readonly IDiagnosesService service;

        public DiagnosesController(IDiagnosesService service)
        {
            this.service = service;
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(IEnumerable<DiagnosisDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPatient([FromRoute] string patientId, CancellationToken ct)
        {
            IEnumerable<DiagnosisDto> list = await service.GetByPatientAsync(patientId, ct);
            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(DiagnosisDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateDiagnosisRequest request, CancellationToken ct)
        {
            int? createdBy = User.GetUserId();
            DiagnosisDto created = await service.CreateAsync(request, createdBy, ct);
            return CreatedAtAction(nameof(GetByPatient), new { patientId = request.PatientId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "professional,admin")]
        [ProducesResponseType(typeof(DiagnosisDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateDiagnosisRequest request, CancellationToken ct)
        {
            DiagnosisDto updated = await service.UpdateAsync(id, request, ct);
            return Ok(updated);
        }
    }
}
