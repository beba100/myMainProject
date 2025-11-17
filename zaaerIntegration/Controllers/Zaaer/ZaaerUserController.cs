using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
	[ApiController]
	[Route("api/zaaer/[controller]")]
	public class ZaaerUserController : ControllerBase
	{
		private readonly IZaaerUserService _userService;
		private readonly ILogger<ZaaerUserController> _logger;
		private readonly IPartnerQueueService _queueService;
		private readonly IQueueSettingsProvider _queueSettings;

		public ZaaerUserController(IZaaerUserService userService, ILogger<ZaaerUserController> logger, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
		{
			_userService = userService;
			_logger = logger;
			_queueService = queueService;
			_queueSettings = queueSettings;
		}

		/// <summary>
		/// Create a new user
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateUser([FromBody] ZaaerCreateUserDto dto)
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
						Operation = "/api/zaaer/ZaaerUser",
						OperationKey = "Zaaer.User.Create",
						PayloadType = nameof(ZaaerCreateUserDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _userService.CreateUserAsync(dto);
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Invalid operation while creating user");
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating user");
				return StatusCode(500, "An error occurred while creating the user.");
			}
		}

		/// <summary>
		/// Update an existing user
		/// </summary>
		[HttpPut]
		public async Task<IActionResult> UpdateUser([FromBody] ZaaerUpdateUserDto dto)
		{
			// Validate that zaaerId is provided
			if (!dto.ZaaerId.HasValue)
			{
				return BadRequest(new { error = "ZaaerId is required for updating user." });
			}

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
						Operation = "/api/zaaer/ZaaerUser",
						OperationKey = "Zaaer.User.Update",
						PayloadType = nameof(ZaaerUpdateUserDto),
						PayloadJson = JsonSerializer.Serialize(dto),
						HotelId = dto.HotelId ?? 0
					};
					await _queueService.EnqueueAsync(q);
					return Accepted(new { queued = true, requestRef = q.RequestRef });
				}
				var result = await _userService.UpdateUserAsync(dto);
				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				_logger.LogWarning(ex, "User not found for update");
				return NotFound(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning(ex, "Invalid operation while updating user");
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating user");
				return StatusCode(500, "An error occurred while updating the user.");
			}
		}

		/// <summary>
		/// Get all users for a specific hotel
		/// </summary>
		[HttpGet("hotel/{hotelId}")]
		public async Task<IActionResult> GetAllUsers(int hotelId)
		{
			try
			{
				var result = await _userService.GetAllUsersAsync(hotelId);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving users for hotel {HotelId}", hotelId);
				return StatusCode(500, "An error occurred while retrieving users.");
			}
		}

		/// <summary>
		/// Get user by ID
		/// </summary>
		[HttpGet("{userId}")]
		public async Task<IActionResult> GetUserById(int userId)
		{
			try
			{
				var result = await _userService.GetUserByIdAsync(userId);
				if (result == null)
					return NotFound($"User with ID {userId} not found");

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving the user.");
			}
		}

		/// <summary>
		/// Delete user (soft delete)
		/// </summary>
		[HttpDelete("{userId}")]
		public async Task<IActionResult> DeleteUser(int userId)
		{
			try
			{
				var result = await _userService.DeleteUserAsync(userId);
				if (!result)
					return NotFound($"User with ID {userId} not found");

				return Ok(new { message = "User deleted successfully" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting user {UserId}", userId);
				return StatusCode(500, "An error occurred while deleting the user.");
			}
		}
	}
}
