using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
	[ApiController]
	[Route("api/zaaer/[controller]")]
	public class ZaaerRoleController : ControllerBase
	{
		private readonly IZaaerRoleService _roleService;
		private readonly ILogger<ZaaerRoleController> _logger;
		private readonly IPartnerQueueService _queueService;
		private readonly IQueueSettingsProvider _queueSettings;

		public ZaaerRoleController(IZaaerRoleService roleService, ILogger<ZaaerRoleController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
		{
			_roleService = roleService;
			_logger = logger;
			_queueService = queueService;
			_queueSettings = queueSettings;
		}

		/// <summary>
		/// Create a new role
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateRole([FromBody] ZaaerCreateRoleDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var q = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/ZaaerRole",
						OperationKey = "Zaaer.Role.Create",
						PayloadType = nameof(ZaaerCreateRoleDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _roleService.CreateRoleAsync(dto);
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Invalid operation while creating role");
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating role");
				return StatusCode(500, "An error occurred while creating the role.");
			}
		}

		/// <summary>
		/// Update an existing role
		/// </summary>
		[HttpPut]
		public async Task<IActionResult> UpdateRole([FromBody] ZaaerUpdateRoleDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var queueSettings = _queueSettings.GetSettings();
				if (queueSettings.EnableQueueMode)
				{
					var q = new EnqueuePartnerRequestDto
					{
						Partner = queueSettings.DefaultPartner,
						Operation = "/api/zaaer/ZaaerRole",
						OperationKey = "Zaaer.Role.Update",
						PayloadType = nameof(ZaaerUpdateRoleDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _roleService.UpdateRoleAsync(dto);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				_logger.LogWarning(ex, "Role not found for update");
				return NotFound(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Invalid operation while updating role");
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating role");
				return StatusCode(500, "An error occurred while updating the role.");
			}
		}

		/// <summary>
		/// Get all roles for a specific hotel
		/// </summary>
		[HttpGet("hotel/{hotelId}")]
		public async Task<IActionResult> GetAllRoles(int hotelId)
		{
			try
			{
				var result = await _roleService.GetAllRolesAsync(hotelId);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving roles for hotel {HotelId}", hotelId);
				return StatusCode(500, "An error occurred while retrieving roles.");
			}
		}

		/// <summary>
		/// Get role by ID
		/// </summary>
		[HttpGet("{roleId}")]
		public async Task<IActionResult> GetRoleById(int roleId)
		{
			try
			{
				var result = await _roleService.GetRoleByIdAsync(roleId);
				if (result == null)
					return NotFound($"Role with ID {roleId} not found");

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving role {RoleId}", roleId);
				return StatusCode(500, "An error occurred while retrieving the role.");
			}
		}

		/// <summary>
		/// Delete role
		/// </summary>
		[HttpDelete("{roleId}")]
		public async Task<IActionResult> DeleteRole(int roleId)
		{
			try
			{
				var result = await _roleService.DeleteRoleAsync(roleId);
				if (!result)
					return NotFound($"Role with ID {roleId} not found");

				return Ok(new { message = "Role deleted successfully" });
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Cannot delete role {RoleId}", roleId);
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting role {RoleId}", roleId);
				return StatusCode(500, "An error occurred while deleting the role.");
			}
		}

		/// <summary>
		/// Get all available permissions
		/// </summary>
		[HttpGet("permissions")]
		public async Task<IActionResult> GetAllPermissions()
		{
			try
			{
				var result = await _roleService.GetAllPermissionsAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving permissions");
				return StatusCode(500, "An error occurred while retrieving permissions.");
			}
		}
	}
}
