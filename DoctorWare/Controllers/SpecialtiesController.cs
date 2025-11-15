using DoctorWare.DTOs.Response;
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
    public class SpecialtiesController : ControllerBase
    {
        private readonly ISpecialtyService specialtyService;

        public SpecialtiesController(ISpecialtyService specialtyService)
        {
            this.specialtyService = specialtyService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SpecialtyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            List<SpecialtyDto> specialties = await specialtyService.GetActiveSpecialtiesAsync(cancellationToken);
            return Ok(specialties);
        }

        [HttpGet("{id:int}/subspecialties")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SubSpecialtyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubSpecialties([FromRoute] int id, CancellationToken cancellationToken)
        {
            List<SubSpecialtyDto> list = await specialtyService.GetSubSpecialtiesAsync(id, cancellationToken);
            return Ok(list);
        }
    }
}
