using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;

namespace zaaerIntegration.Controllers.Zaaer
{
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerShomoosDetailsController : ControllerBase
    {
        private readonly IZaaerShomoosDetailsService _service;

        public ZaaerShomoosDetailsController(IZaaerShomoosDetailsService service)
        { _service = service; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ZaaerCreateShomoosDetailsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut("{detailsId:int}")]
        public async Task<IActionResult> Update([FromRoute] int detailsId, [FromBody] ZaaerUpdateShomoosDetailsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(detailsId, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
    }
}


