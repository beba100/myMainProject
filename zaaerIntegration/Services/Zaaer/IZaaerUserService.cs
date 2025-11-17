using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
	public interface IZaaerUserService
	{
		Task<ZaaerUserResponseDto> CreateUserAsync(ZaaerCreateUserDto dto);
		Task<ZaaerUserResponseDto> UpdateUserAsync(ZaaerUpdateUserDto dto);
		Task<List<ZaaerUserResponseDto>> GetAllUsersAsync(int hotelId);
		Task<ZaaerUserResponseDto?> GetUserByIdAsync(int userId);
		Task<bool> DeleteUserAsync(int userId);
	}
}
