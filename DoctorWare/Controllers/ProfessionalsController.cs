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
    public class ProfessionalsController : ControllerBase
    {
        private readonly IProfessionalsService professionalsService;

        public ProfessionalsController(IProfessionalsService professionalsService)
        {
            this.professionalsService = professionalsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] int? specialtyId, [FromQuery] string? name, CancellationToken ct)
        {
            IEnumerable<ProfessionalListItemDto> list = await professionalsService.GetAsync(specialtyId, name, ct);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            ProfessionalListItemDto? item = await professionalsService.GetByIdAsync(id, ct);
            if (item is null)
            {
                return NotFound(new { message = "Profesional no encontrado" });
            }
            return Ok(item);
        }
    }
}
