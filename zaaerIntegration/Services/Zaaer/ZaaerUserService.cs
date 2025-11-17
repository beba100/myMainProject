using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace zaaerIntegration.Services.Zaaer
{
	public class ZaaerUserService : IZaaerUserService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ZaaerUserService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<ZaaerUserResponseDto> CreateUserAsync(ZaaerCreateUserDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// Check if user with same ZaaerId already exists
				if (dto.ZaaerId.HasValue)
				{
					var existingZaaerUser = await _unitOfWork.Users.FindSingleAsync(u => u.ZaaerId.HasValue && u.ZaaerId.Value == dto.ZaaerId.Value);
					if (existingZaaerUser != null)
					{
						throw new InvalidOperationException($"User with Zaaer ID {dto.ZaaerId} already exists. Use PUT endpoint to update instead.");
					}
				}

				// Check if email already exists
				var existingUser = await _unitOfWork.Users.FindSingleAsync(u => u.Email == dto.Email && u.HotelId == dto.HotelId);
				if (existingUser != null)
				{
					throw new InvalidOperationException($"User with email '{dto.Email}' already exists in this hotel.");
				}

				// Hash the password (use default password if not provided)
				var password = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : "Za@er123"; // Default password if not provided
				var passwordHash = HashPassword(password);

				var user = new User
				{
					HotelId = dto.HotelId,
					FirstName = dto.FirstName,
					LastName = dto.LastName,
					Title = dto.Title,
					ProfilePictureUrl = dto.ProfilePictureUrl,
					SignatureUrl = dto.SignatureUrl,
					DateOfBirth = dto.DateOfBirth,
					Gender = dto.Gender,
					Department = dto.Department,
					Description = dto.Description,
					Email = dto.Email,
					PhoneNumber = dto.PhoneNumber,
					BusinessPhoneNumber = dto.BusinessPhoneNumber,
					Address = dto.Address,
					PasswordHash = passwordHash,
					UserType = dto.UserType,
					RoleId = dto.RoleId,
					Status = dto.Status,
					ChangePassword = dto.ChangePassword,
					ZaaerId = dto.ZaaerId,
					CreatedAt = KsaTime.Now,
					IsActive = true
				};

				await _unitOfWork.Users.AddAsync(user);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return await GetUserResponseDtoAsync(user);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<ZaaerUserResponseDto> UpdateUserAsync(ZaaerUpdateUserDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// Find user by ZaaerId (preferred) or fallback to other methods
				User? user = null;
				if (dto.ZaaerId.HasValue)
				{
					user = await _unitOfWork.Users.FindSingleAsync(u => u.ZaaerId.HasValue && u.ZaaerId.Value == dto.ZaaerId.Value);
				}
				
				if (user == null)
				{
					throw new KeyNotFoundException($"User with Zaaer ID {dto.ZaaerId} not found");
				}

				// Check if email already exists for another user (only if email is being updated)
				if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
				{
					var hotelId = dto.HotelId ?? user.HotelId;
					var existingUser = await _unitOfWork.Users.FindSingleAsync(u => u.Email == dto.Email && u.HotelId == hotelId && u.UserId != user.UserId);
					if (existingUser != null)
					{
						throw new InvalidOperationException($"User with email '{dto.Email}' already exists in this hotel.");
					}
				}

				// Update user properties only if provided
				if (!string.IsNullOrEmpty(dto.FirstName))
					user.FirstName = dto.FirstName;
				if (!string.IsNullOrEmpty(dto.LastName))
					user.LastName = dto.LastName;
				if (dto.Title != null)
					user.Title = dto.Title;
				if (dto.ProfilePictureUrl != null)
					user.ProfilePictureUrl = dto.ProfilePictureUrl;
				if (dto.SignatureUrl != null)
					user.SignatureUrl = dto.SignatureUrl;
				if (dto.DateOfBirth.HasValue)
					user.DateOfBirth = dto.DateOfBirth;
				if (dto.Gender != null)
					user.Gender = dto.Gender;
				if (dto.Department != null)
					user.Department = dto.Department;
				if (dto.Description != null)
					user.Description = dto.Description;
				if (!string.IsNullOrEmpty(dto.Email))
					user.Email = dto.Email;
				if (dto.PhoneNumber != null)
					user.PhoneNumber = dto.PhoneNumber;
				if (dto.BusinessPhoneNumber != null)
					user.BusinessPhoneNumber = dto.BusinessPhoneNumber;
				if (dto.Address != null)
					user.Address = dto.Address;
				if (dto.UserType != null)
					user.UserType = dto.UserType;
				if (dto.RoleId.HasValue)
					user.RoleId = dto.RoleId;
				if (dto.Status.HasValue)
					user.Status = dto.Status.Value;
				if (dto.ChangePassword.HasValue)
					user.ChangePassword = dto.ChangePassword.Value;
				if (dto.HotelId.HasValue)
					user.HotelId = dto.HotelId.Value;
				if (dto.ZaaerId.HasValue)
					user.ZaaerId = dto.ZaaerId.Value;
				
				user.UpdatedAt = KsaTime.Now;

				// Update password if provided
				if (!string.IsNullOrEmpty(dto.Password))
				{
					user.PasswordHash = HashPassword(dto.Password);
				}

				_unitOfWork.Users.Update(user);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return await GetUserResponseDtoAsync(user);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<List<ZaaerUserResponseDto>> GetAllUsersAsync(int hotelId)
		{
			var users = await _unitOfWork.Users.FindAsync(u => u.HotelId == hotelId);
			var result = new List<ZaaerUserResponseDto>();

			foreach (var user in users)
			{
				result.Add(await GetUserResponseDtoAsync(user));
			}

			return result;
		}

		public async Task<ZaaerUserResponseDto?> GetUserByIdAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null)
				return null;

			return await GetUserResponseDtoAsync(user);
		}

		public async Task<bool> DeleteUserAsync(int userId)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var user = await _unitOfWork.Users.GetByIdAsync(userId);
				if (user == null)
					return false;

				// Soft delete by setting IsActive to false
				user.IsActive = false;
				user.UpdatedAt = KsaTime.Now;

				_unitOfWork.Users.Update(user);
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

		private async Task<ZaaerUserResponseDto> GetUserResponseDtoAsync(User user)
		{
			var roleName = string.Empty;
			if (user.RoleId.HasValue)
			{
				var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
				roleName = role?.RoleName ?? string.Empty;
			}

			return new ZaaerUserResponseDto
			{
				UserId = user.UserId,
				HotelId = user.HotelId,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Title = user.Title,
				ProfilePictureUrl = user.ProfilePictureUrl,
				SignatureUrl = user.SignatureUrl,
				DateOfBirth = user.DateOfBirth,
				Gender = user.Gender,
				Department = user.Department,
				Description = user.Description,
				Email = user.Email,
				PhoneNumber = user.PhoneNumber,
				BusinessPhoneNumber = user.BusinessPhoneNumber,
				Address = user.Address,
				UserType = user.UserType,
				RoleId = user.RoleId,
				RoleName = roleName,
				Status = user.Status,
				ChangePassword = user.ChangePassword,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				LastLogin = user.LastLogin,
				IsActive = user.IsActive
			};
		}

		private string HashPassword(string password)
		{
			using var sha256 = SHA256.Create();
			var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
			return Convert.ToBase64String(hashedBytes);
		}
	}
}
