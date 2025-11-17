using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
	public interface IZaaerRoleService
	{
		Task<ZaaerRoleResponseDto> CreateRoleAsync(ZaaerCreateRoleDto dto);
		Task<ZaaerRoleResponseDto> UpdateRoleAsync(ZaaerUpdateRoleDto dto);
		Task<List<ZaaerRoleResponseDto>> GetAllRolesAsync(int hotelId);
		Task<ZaaerRoleResponseDto?> GetRoleByIdAsync(int roleId);
		Task<bool> DeleteRoleAsync(int roleId);
		Task<List<ZaaerPermissionResponseDto>> GetAllPermissionsAsync();
	}
}
