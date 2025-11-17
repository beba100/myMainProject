using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
	/// <summary>
	/// Interface for Zaaer Tax Service
	/// </summary>
	public interface IZaaerTaxService
	{
		Task<ZaaerTaxResponseDto> CreateTaxAsync(ZaaerCreateTaxDto createTaxDto);
		Task<ZaaerTaxResponseDto?> UpdateTaxAsync(int zaaerId, ZaaerUpdateTaxDto updateTaxDto);
		Task<bool> DeleteTaxAsync(int zaaerId);
	}

	/// <summary>
	/// Service for Zaaer Tax operations
	/// </summary>
	public class ZaaerTaxService : IZaaerTaxService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<ZaaerTaxService> _logger;

		public ZaaerTaxService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			ILogger<ZaaerTaxService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		/// <summary>
		/// Create a new tax record
		/// </summary>
		public async Task<ZaaerTaxResponseDto> CreateTaxAsync(ZaaerCreateTaxDto createTaxDto)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Check if tax with same ZaaerId already exists
				if (createTaxDto.ZaaerId.HasValue)
				{
					var existingTax = await _unitOfWork.Taxes.FindSingleAsync(t => t.ZaaerId.HasValue && t.ZaaerId.Value == createTaxDto.ZaaerId.Value && t.HotelId == createTaxDto.HotelId);
					if (existingTax != null)
					{
						throw new InvalidOperationException($"Tax with Zaaer ID {createTaxDto.ZaaerId} already exists for this hotel. Use PUT endpoint to update instead.");
					}
				}

				var tax = _mapper.Map<Tax>(createTaxDto);
				tax.CreatedAt = KsaTime.Now;
				tax.Enabled = createTaxDto.Enabled;
				// Set TaxId to Id after creation if not provided
				if (!tax.TaxId.HasValue && createTaxDto.TaxId.HasValue)
				{
					tax.TaxId = createTaxDto.TaxId.Value;
				}

				await _unitOfWork.Taxes.AddAsync(tax);
				await _unitOfWork.SaveChangesAsync();
				
				// If TaxId was not provided, set it to the generated Id
				if (!tax.TaxId.HasValue)
				{
					tax.TaxId = tax.Id;
					await _unitOfWork.Taxes.UpdateAsync(tax);
				}
				
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return _mapper.Map<ZaaerTaxResponseDto>(tax);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating tax record for ZaaerId={ZaaerId}", createTaxDto.ZaaerId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		/// <summary>
		/// Update an existing tax record by Zaaer ID
		/// </summary>
		public async Task<ZaaerTaxResponseDto?> UpdateTaxAsync(int zaaerId, ZaaerUpdateTaxDto updateTaxDto)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var tax = await _unitOfWork.Taxes.FindSingleAsync(t => t.ZaaerId.HasValue && t.ZaaerId.Value == zaaerId);
				if (tax == null)
				{
					return null;
				}

				// Update only provided fields
				if (!string.IsNullOrEmpty(updateTaxDto.TaxName))
					tax.TaxName = updateTaxDto.TaxName;
				if (!string.IsNullOrEmpty(updateTaxDto.TaxType))
					tax.TaxType = updateTaxDto.TaxType;
				if (updateTaxDto.TaxRate.HasValue)
					tax.TaxRate = updateTaxDto.TaxRate.Value;
				if (!string.IsNullOrEmpty(updateTaxDto.Method))
					tax.Method = updateTaxDto.Method;
				if (updateTaxDto.Enabled.HasValue)
					tax.Enabled = updateTaxDto.Enabled.Value;
				if (!string.IsNullOrEmpty(updateTaxDto.TaxCode))
					tax.TaxCode = updateTaxDto.TaxCode;
				if (updateTaxDto.ApplyOn != null)
					tax.ApplyOn = updateTaxDto.ApplyOn;
				if (updateTaxDto.TaxId.HasValue)
					tax.TaxId = updateTaxDto.TaxId.Value;
				if (updateTaxDto.HotelId.HasValue)
					tax.HotelId = updateTaxDto.HotelId.Value;
				if (updateTaxDto.ZaaerId.HasValue)
					tax.ZaaerId = updateTaxDto.ZaaerId.Value;

				tax.UpdatedAt = KsaTime.Now;

				await _unitOfWork.Taxes.UpdateAsync(tax);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return _mapper.Map<ZaaerTaxResponseDto>(tax);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating tax record for ZaaerId={ZaaerId}", zaaerId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		/// <summary>
		/// Delete a tax record by Zaaer ID (soft delete - updates enabled to false)
		/// </summary>
		public async Task<bool> DeleteTaxAsync(int zaaerId)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var tax = await _unitOfWork.Taxes.FindSingleAsync(t => t.ZaaerId.HasValue && t.ZaaerId.Value == zaaerId);
				if (tax == null)
				{
					_logger.LogWarning("Tax record with ZaaerId {ZaaerId} not found for deletion", zaaerId);
					await _unitOfWork.RollbackTransactionAsync();
					return false;
				}

				// Soft delete: Update enabled to false instead of actually deleting
				tax.Enabled = false;
				tax.UpdatedAt = KsaTime.Now;

				await _unitOfWork.Taxes.UpdateAsync(tax);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Tax record with ZaaerId {ZaaerId} soft deleted successfully", zaaerId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting tax record for ZaaerId={ZaaerId}. Error: {ErrorMessage}", zaaerId, ex.Message);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}
	}
}

