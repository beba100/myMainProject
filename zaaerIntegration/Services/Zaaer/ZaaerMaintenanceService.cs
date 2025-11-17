using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
	/// <summary>
	/// Interface for Zaaer Maintenance Service
	/// </summary>
	public interface IZaaerMaintenanceService
	{
		Task<ZaaerMaintenanceResponseDto> CreateMaintenanceAsync(ZaaerCreateMaintenanceDto createMaintenanceDto);
		Task<ZaaerMaintenanceResponseDto?> UpdateMaintenanceAsync(int zaaerId, ZaaerUpdateMaintenanceDto updateMaintenanceDto);
		Task<bool> DeleteMaintenanceAsync(int zaaerId);
	}

	/// <summary>
	/// Service for Zaaer Maintenance operations
	/// </summary>
	public class ZaaerMaintenanceService : IZaaerMaintenanceService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<ZaaerMaintenanceService> _logger;

		public ZaaerMaintenanceService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			ILogger<ZaaerMaintenanceService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		/// <summary>
		/// Create a new maintenance record
		/// </summary>
		public async Task<ZaaerMaintenanceResponseDto> CreateMaintenanceAsync(ZaaerCreateMaintenanceDto createMaintenanceDto)
		{
			try
			{
				var maintenance = _mapper.Map<Maintenance>(createMaintenanceDto);
				maintenance.CreatedAt = KsaTime.Now;
				maintenance.Status = "active";

				// Set apartment status to "maintenance" when creating maintenance record
				if (maintenance.UnitId > 0)
				{
					var apartment = await _unitOfWork.Apartments.GetByIdAsync(maintenance.UnitId);
					if (apartment != null)
					{
						apartment.Status = "maintenance";
						await _unitOfWork.Apartments.UpdateAsync(apartment);
					}
				}

				await _unitOfWork.Maintenances.AddAsync(maintenance);
				await _unitOfWork.SaveChangesAsync();

				return _mapper.Map<ZaaerMaintenanceResponseDto>(maintenance);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating maintenance record");
				throw;
			}
		}

		/// <summary>
		/// Update an existing maintenance record by Zaaer ID
		/// </summary>
		public async Task<ZaaerMaintenanceResponseDto?> UpdateMaintenanceAsync(int zaaerId, ZaaerUpdateMaintenanceDto updateMaintenanceDto)
		{
			try
			{
				var maintenances = await _unitOfWork.Maintenances.FindAsync(m => m.ZaaerId == zaaerId);
				var maintenance = maintenances.FirstOrDefault();

				if (maintenance == null)
				{
					return null;
				}

				_mapper.Map(updateMaintenanceDto, maintenance);
				maintenance.UpdatedAt = KsaTime.Now;

				await _unitOfWork.Maintenances.UpdateAsync(maintenance);
				await _unitOfWork.SaveChangesAsync();

				return _mapper.Map<ZaaerMaintenanceResponseDto>(maintenance);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating maintenance record with ZaaerId {ZaaerId}", zaaerId);
				throw;
			}
		}

		/// <summary>
		/// Delete a maintenance record by Zaaer ID and set apartment status to "vacant"
		/// </summary>
		public async Task<bool> DeleteMaintenanceAsync(int zaaerId)
		{
			try
			{
				var maintenances = await _unitOfWork.Maintenances.FindAsync(m => m.ZaaerId == zaaerId);
				var maintenance = maintenances.FirstOrDefault();

				if (maintenance == null)
				{
					return false;
				}

				// Set apartment status to "vacant" when deleting maintenance record
				if (maintenance.UnitId > 0)
				{
					var apartment = await _unitOfWork.Apartments.GetByIdAsync(maintenance.UnitId);
					if (apartment != null)
					{
						apartment.Status = "vacant";
						await _unitOfWork.Apartments.UpdateAsync(apartment);
					}
				}

				await _unitOfWork.Maintenances.DeleteAsync(maintenance);
				await _unitOfWork.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting maintenance record with ZaaerId {ZaaerId}", zaaerId);
				throw;
			}
		}
	}
}

