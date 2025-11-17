using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
	public class ZaaerRoleService : IZaaerRoleService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ZaaerRoleService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<ZaaerRoleResponseDto> CreateRoleAsync(ZaaerCreateRoleDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// Check if role name already exists in this hotel
				var existingRole = await _unitOfWork.Roles.FindSingleAsync(r => r.RoleName == dto.RoleName && r.HotelId == dto.HotelId);
				if (existingRole != null)
				{
					throw new InvalidOperationException($"Role with name '{dto.RoleName}' already exists in this hotel.");
				}

				var role = new Role
				{
					HotelId = dto.HotelId,
					RoleName = dto.RoleName,
					RoleDescription = dto.RoleDescription,
					IsActive = dto.IsActive,
					CreatedAt = KsaTime.Now
				};

				await _unitOfWork.Roles.AddAsync(role);
				await _unitOfWork.SaveChangesAsync();

				// Add permissions to the role
				if (dto.PermissionIds.Any())
				{
					await AddPermissionsToRoleAsync(role.RoleId, dto.PermissionIds);
				}

				await _unitOfWork.CommitTransactionAsync();

				return await GetRoleResponseDtoAsync(role);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<ZaaerRoleResponseDto> UpdateRoleAsync(ZaaerUpdateRoleDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var role = await _unitOfWork.Roles.GetByIdAsync(dto.RoleId);
				if (role == null)
				{
					throw new KeyNotFoundException($"Role with ID {dto.RoleId} not found");
				}

				// Check if role name already exists for another role in this hotel
				var existingRole = await _unitOfWork.Roles.FindSingleAsync(r => r.RoleName == dto.RoleName && r.HotelId == dto.HotelId && r.RoleId != dto.RoleId);
				if (existingRole != null)
				{
					throw new InvalidOperationException($"Role with name '{dto.RoleName}' already exists in this hotel.");
				}

				// Update role properties
				role.RoleName = dto.RoleName;
				role.RoleDescription = dto.RoleDescription;
				role.IsActive = dto.IsActive;
				role.UpdatedAt = KsaTime.Now;

				_unitOfWork.Roles.Update(role);

				// Update permissions
				await UpdateRolePermissionsAsync(role.RoleId, dto.PermissionIds);

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return await GetRoleResponseDtoAsync(role);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<List<ZaaerRoleResponseDto>> GetAllRolesAsync(int hotelId)
		{
			var roles = await _unitOfWork.Roles.FindAsync(r => r.HotelId == hotelId);
			var result = new List<ZaaerRoleResponseDto>();

			foreach (var role in roles)
			{
				result.Add(await GetRoleResponseDtoAsync(role));
			}

			return result;
		}

		public async Task<ZaaerRoleResponseDto?> GetRoleByIdAsync(int roleId)
		{
			var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
			if (role == null)
				return null;

			return await GetRoleResponseDtoAsync(role);
		}

		public async Task<bool> DeleteRoleAsync(int roleId)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
				if (role == null)
					return false;

				// Check if any users are assigned to this role
				var usersWithRole = await _unitOfWork.Users.FindAsync(u => u.RoleId == roleId);
				if (usersWithRole.Any())
				{
					throw new InvalidOperationException($"Cannot delete role '{role.RoleName}' because {usersWithRole.Count()} user(s) are assigned to it.");
				}

				// Delete role permissions first
				var rolePermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == roleId);
				foreach (var rp in rolePermissions)
				{
					_unitOfWork.RolePermissions.Delete(rp);
				}

				// Delete the role
				_unitOfWork.Roles.Delete(role);

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return true;
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<List<ZaaerPermissionResponseDto>> GetAllPermissionsAsync()
		{
			var permissions = await _unitOfWork.Permissions.GetAllAsync();
			return permissions.Select(p => new ZaaerPermissionResponseDto
			{
				PermissionId = p.PermissionId,
				PermissionName = p.PermissionName,
				PermissionCode = p.PermissionCode,
				ModuleName = p.ModuleName,
				ActionName = p.ActionName,
				Description = p.Description,
				IsActive = p.IsActive,
				Granted = false // Default value, will be set based on role context
			}).ToList();
		}

		private async Task AddPermissionsToRoleAsync(int roleId, List<int> permissionIds)
		{
			foreach (var permissionId in permissionIds)
			{
				var rolePermission = new RolePermission
				{
					RoleId = roleId,
					PermissionId = permissionId,
					Granted = true,
					CreatedAt = KsaTime.Now
				};

				await _unitOfWork.RolePermissions.AddAsync(rolePermission);
			}
		}

		private async Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
		{
			// Remove existing permissions
			var existingPermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == roleId);
			foreach (var rp in existingPermissions)
			{
				_unitOfWork.RolePermissions.Delete(rp);
			}

			// Add new permissions
			await AddPermissionsToRoleAsync(roleId, permissionIds);
		}

		private async Task<ZaaerRoleResponseDto> GetRoleResponseDtoAsync(Role role)
		{
			// Get role permissions
			var rolePermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == role.RoleId);
			var permissions = new List<ZaaerPermissionResponseDto>();

			foreach (var rp in rolePermissions)
			{
				var permission = await _unitOfWork.Permissions.GetByIdAsync(rp.PermissionId);
				if (permission != null)
				{
					permissions.Add(new ZaaerPermissionResponseDto
					{
						PermissionId = permission.PermissionId,
						PermissionName = permission.PermissionName,
						PermissionCode = permission.PermissionCode,
						ModuleName = permission.ModuleName,
						ActionName = permission.ActionName,
						Description = permission.Description,
						IsActive = permission.IsActive,
						Granted = rp.Granted
					});
				}
			}

			return new ZaaerRoleResponseDto
			{
				RoleId = role.RoleId,
				HotelId = role.HotelId,
				RoleName = role.RoleName,
				RoleDescription = role.RoleDescription,
				IsActive = role.IsActive,
				CreatedAt = role.CreatedAt,
				UpdatedAt = role.UpdatedAt,
				CreatedBy = role.CreatedBy,
				UpdatedBy = role.UpdatedBy,
				Permissions = permissions
			};
		}
	}
}
