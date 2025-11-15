using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;
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
    public class AllergiesController : ControllerBase
    {
        private readonly IAllergiesService service;

        public AllergiesController(IAllergiesService service)
        {
            this.service = service;
        }

        [HttpGet("patient/{patientId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AllergyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPatient([FromRoute] string patientId, CancellationToken ct)
        {
            IEnumerable<AllergyDto> list = await service.GetByPatientAsync(patientId, ct);
            return Ok(list);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(AllergyDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateAllergyRequest request, CancellationToken ct)
        {
            AllergyDto created = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetByPatient), new { patientId = request.PatientId }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AllergyDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateAllergyRequest request, CancellationToken ct)
        {
            AllergyDto updated = await service.UpdateAsync(id, request, ct);
            return Ok(updated);
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> Deactivate([FromRoute] string id, CancellationToken ct)
        {
            bool ok = await service.DeactivateAsync(id, ct);
            return Ok(new { success = ok });
        }
    }
}

