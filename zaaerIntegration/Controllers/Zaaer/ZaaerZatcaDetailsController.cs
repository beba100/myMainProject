using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;

namespace zaaerIntegration.Controllers.Zaaer
{
	[ApiController]
	[Route("api/zaaer/[controller]")]
	public class ZaaerZatcaDetailsController : ControllerBase
	{
		private readonly IZaaerZatcaDetailsService _service;
		private readonly ILogger<ZaaerZatcaDetailsController> _logger;

		public ZaaerZatcaDetailsController(IZaaerZatcaDetailsService service, ILogger<ZaaerZatcaDetailsController> logger)
		{ _service = service; _logger = logger; }

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] ZaaerCreateZatcaDetailsDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var result = await _service.CreateAsync(dto);
			return Ok(result);
		}

		[HttpPut("{detailsId:int}")]
		public async Task<IActionResult> Update([FromRoute] int detailsId, [FromBody] ZaaerUpdateZatcaDetailsDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var result = await _service.UpdateAsync(detailsId, dto);
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpGet("hotel/{hotelId:int}")]
		public async Task<IActionResult> GetByHotel([FromRoute] int hotelId)
		{
			var list = await _service.GetAllByHotelIdAsync(hotelId);
			return Ok(list);
		}

		[HttpGet("{detailsId:int}")]
		public async Task<IActionResult> GetById([FromRoute] int detailsId)
		{
			var result = await _service.GetByIdAsync(detailsId);
			if (result == null) return NotFound();
			return Ok(result);
		}
	}
}


