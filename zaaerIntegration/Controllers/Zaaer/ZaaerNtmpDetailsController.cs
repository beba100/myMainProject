using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;

namespace zaaerIntegration.Controllers.Zaaer
{
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerNtmpDetailsController : ControllerBase
    {
        private readonly IZaaerNtmpDetailsService _service;

        public ZaaerNtmpDetailsController(IZaaerNtmpDetailsService service)
        { _service = service; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ZaaerCreateNtmpDetailsDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut("{detailsId:int}")]
        public async Task<IActionResult> Update([FromRoute] int detailsId, [FromBody] ZaaerUpdateNtmpDetailsDto dto)
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


