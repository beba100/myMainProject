using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
	/// <summary>
	/// ZaaerRoleService is disabled - Role model is now in Master DB only
	/// Use MasterUserService and Master DB for role management instead
	/// </summary>
	public class ZaaerRoleService : IZaaerRoleService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ZaaerRoleService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<ZaaerRoleResponseDto> CreateRoleAsync(ZaaerCreateRoleDto dto)
		{
			// Role model is now in Master DB only, not in Tenant DB
			// Use MasterUserService and Master DB for role management instead
			throw new NotImplementedException("ZaaerRoleService is disabled. Role management is now handled in Master DB. Use MasterUserService instead.");
		}

		public async Task<ZaaerRoleResponseDto> UpdateRoleAsync(ZaaerUpdateRoleDto dto)
		{
			// Role model is now in Master DB only, not in Tenant DB
			// Use MasterUserService and Master DB for role management instead
			throw new NotImplementedException("ZaaerRoleService is disabled. Role management is now handled in Master DB. Use MasterUserService instead.");
		}

		public async Task<List<ZaaerRoleResponseDto>> GetAllRolesAsync(int hotelId)
		{
			// Role model is now in Master DB only, not in Tenant DB
			// Use MasterUserService and Master DB for role management instead
			throw new NotImplementedException("ZaaerRoleService is disabled. Role management is now handled in Master DB. Use MasterUserService instead.");
		}

		public async Task<ZaaerRoleResponseDto?> GetRoleByIdAsync(int roleId)
		{
			// Role model is now in Master DB only, not in Tenant DB
			// Use MasterUserService and Master DB for role management instead
			throw new NotImplementedException("ZaaerRoleService is disabled. Role management is now handled in Master DB. Use MasterUserService instead.");
		}

		public async Task<bool> DeleteRoleAsync(int roleId)
		{
			// Role model is now in Master DB only, not in Tenant DB
			// Use MasterUserService and Master DB for role management instead
			throw new NotImplementedException("ZaaerRoleService is disabled. Role management is now handled in Master DB. Use MasterUserService instead.");
		}

		public async Task<List<ZaaerPermissionResponseDto>> GetAllPermissionsAsync()
		{
			// Role model is now in Master DB only, not in Tenant DB
			// Use MasterUserService and Master DB for role management instead
			throw new NotImplementedException("ZaaerRoleService is disabled. Role management is now handled in Master DB. Use MasterUserService instead.");
		}

		// Private methods removed - service is disabled
		// private async Task AddPermissionsToRoleAsync(int roleId, List<int> permissionIds) { }
		// private async Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds) { }
		// private async Task<ZaaerRoleResponseDto> GetRoleResponseDtoAsync(Role role) { }
	}
}
