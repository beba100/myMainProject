using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;

namespace zaaerIntegration.Controllers.Zaaer
{
    [ApiController]
    [Route("api/zaaer/[controller]")]
    public class ZaaerIntegrationResponsesController : ControllerBase
    {
        private readonly IZaaerIntegrationResponseService _service;

        public ZaaerIntegrationResponsesController(IZaaerIntegrationResponseService service)
        { _service = service; }

        // POST: api/zaaer/ZaaerIntegrationResponses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ZaaerCreateIntegrationResponseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        // GET: api/zaaer/ZaaerIntegrationResponses
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] ZaaerIntegrationResponseQuery query)
        {
            var list = await _service.SearchAsync(query);
            return Ok(list);
        }

        // PUT: api/zaaer/ZaaerIntegrationResponses/{responseId}
        [HttpPut("{responseId:int}")]
        public async Task<IActionResult> Update([FromRoute] int responseId, [FromBody] ZaaerCreateIntegrationResponseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(responseId, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}


