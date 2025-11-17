using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
	public interface IZaaerRateTypeService
	{
		Task<ZaaerRateTypeResponseDto> CreateAsync(ZaaerCreateRateTypeDto dto);
		Task<ZaaerRateTypeResponseDto?> UpdateAsync(int rateTypeId, ZaaerUpdateRateTypeDto dto);
		Task<ZaaerRateTypeResponseDto?> UpdateByZaaerIdAsync(int zaaerId, ZaaerUpdateRateTypeDto dto);
		Task<ZaaerRateTypeResponseDto?> GetByIdAsync(int rateTypeId);
		Task<IEnumerable<ZaaerRateTypeResponseDto>> GetAllByHotelIdAsync(int hotelId);
		Task<bool> DeleteByZaaerIdAsync(int zaaerId);
	}

	public class ZaaerRateTypeService : IZaaerRateTypeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public ZaaerRateTypeService(IUnitOfWork unitOfWork, ApplicationDbContext context, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_context = context;
			_mapper = mapper;
		}

		public async Task<ZaaerRateTypeResponseDto> CreateAsync(ZaaerCreateRateTypeDto dto)
		{
			// Load existing rate type and its unit items manually (no FK constraint)
			var existing = await _context.RateTypes
				.FirstOrDefaultAsync(r =>
					(dto.ZaaerId.HasValue && r.ZaaerId == dto.ZaaerId.Value) ||
					(r.HotelId == dto.HotelId && r.ShortCode == dto.ShortCode));

			if (existing != null)
			{
				existing.HotelId = dto.HotelId;
				existing.ShortCode = dto.ShortCode;
				existing.Title = dto.Title;
				existing.Status = dto.Status;
				existing.ZaaerId = dto.ZaaerId;
				existing.UpdatedAt = KsaTime.Now;

				// Load existing unit items manually (no FK constraint, so manual loading)
				var existingItems = await _context.RateTypeUnitItems
					.Where(i => i.RateTypeId == existing.Id)
					.ToListAsync();

				var itemsByName = existingItems.ToDictionary(i => i.UnitTypeName, StringComparer.OrdinalIgnoreCase);
				foreach (var itemDto in dto.UnitItems)
				{
					if (itemsByName.TryGetValue(itemDto.UnitTypeName, out var item))
					{
						// Update existing item
						item.Rate = itemDto.Rate;
						item.IsEnabled = itemDto.IsEnabled;
						item.UpdatedAt = KsaTime.Now;
						await _unitOfWork.RateTypeUnitItems.UpdateAsync(item);
					}
					else
					{
						// Add new item using repository
						var newItem = _mapper.Map<RateTypeUnitItem>(itemDto);
						newItem.RateTypeId = existing.Id;
						await _unitOfWork.RateTypeUnitItems.AddAsync(newItem);
					}
				}

				await _unitOfWork.RateTypes.UpdateAsync(existing);
				await _unitOfWork.SaveChangesAsync();
				return await Map(existing.Id);
			}

			var entity = _mapper.Map<RateType>(dto);
			await _unitOfWork.RateTypes.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			foreach (var itemDto in dto.UnitItems)
			{
				var item = _mapper.Map<RateTypeUnitItem>(itemDto);
				item.RateTypeId = entity.Id;
				await _unitOfWork.RateTypeUnitItems.AddAsync(item);
			}
			await _unitOfWork.SaveChangesAsync();

			return await Map(entity.Id);
		}

	public async Task<ZaaerRateTypeResponseDto?> UpdateAsync(int rateTypeId, ZaaerUpdateRateTypeDto dto)
	{
		var entity = await _context.RateTypes.FirstOrDefaultAsync(r => r.Id == rateTypeId);
		if (entity == null) return null;

		if (dto.HotelId.HasValue) entity.HotelId = dto.HotelId.Value;
		if (!string.IsNullOrWhiteSpace(dto.ShortCode)) entity.ShortCode = dto.ShortCode!;
		if (!string.IsNullOrWhiteSpace(dto.Title)) entity.Title = dto.Title!;
		if (dto.Status.HasValue) entity.Status = dto.Status.Value;
		if (dto.ZaaerId.HasValue) entity.ZaaerId = dto.ZaaerId.Value;
		entity.UpdatedAt = KsaTime.Now;

		// Load existing unit items manually (no FK constraint, so manual loading)
		var existingItems = await _context.RateTypeUnitItems
			.Where(i => i.RateTypeId == rateTypeId)
			.ToListAsync();

		// Upsert items by UnitTypeName
		var itemsByName = existingItems.ToDictionary(i => i.UnitTypeName, StringComparer.OrdinalIgnoreCase);
		foreach (var itemDto in dto.UnitItems)
		{
			if (itemsByName.TryGetValue(itemDto.UnitTypeName, out var existingItem))
			{
				// Update existing item
				existingItem.Rate = itemDto.Rate;
				existingItem.IsEnabled = itemDto.IsEnabled;
				existingItem.UpdatedAt = KsaTime.Now;
				await _unitOfWork.RateTypeUnitItems.UpdateAsync(existingItem);
			}
			else
			{
				// Add new item using repository
				var newItem = _mapper.Map<RateTypeUnitItem>(itemDto);
				newItem.RateTypeId = rateTypeId;
				await _unitOfWork.RateTypeUnitItems.AddAsync(newItem);
			}
		}

		await _unitOfWork.RateTypes.UpdateAsync(entity);
		await _unitOfWork.SaveChangesAsync();
		return await Map(entity.Id);
	}

	public async Task<ZaaerRateTypeResponseDto?> UpdateByZaaerIdAsync(int zaaerId, ZaaerUpdateRateTypeDto dto)
	{
		// Find rate type by ZaaerId
		var query = _context.RateTypes.Where(r => r.ZaaerId == zaaerId);

		// Optionally filter by HotelId if provided
		if (dto.HotelId.HasValue)
		{
			query = query.Where(r => r.HotelId == dto.HotelId.Value);
		}

		var entity = await query.FirstOrDefaultAsync();
		if (entity == null) return null;

		// Update entity properties
		if (dto.HotelId.HasValue) entity.HotelId = dto.HotelId.Value;
		if (!string.IsNullOrWhiteSpace(dto.ShortCode)) entity.ShortCode = dto.ShortCode!;
		if (!string.IsNullOrWhiteSpace(dto.Title)) entity.Title = dto.Title!;
		if (dto.Status.HasValue) entity.Status = dto.Status.Value;
		entity.ZaaerId = zaaerId; // Ensure zaaerId is set
		entity.UpdatedAt = KsaTime.Now;

		var rateTypeId = entity.Id;

		// Load existing unit items manually (no FK constraint, so manual loading)
		var existingItems = await _context.RateTypeUnitItems
			.Where(i => i.RateTypeId == rateTypeId)
			.ToListAsync();

		// Upsert items by UnitTypeName (only if UnitItems is provided and not empty)
		if (dto.UnitItems != null && dto.UnitItems.Count > 0)
		{
			var itemsByName = existingItems.ToDictionary(i => i.UnitTypeName, StringComparer.OrdinalIgnoreCase);
			foreach (var itemDto in dto.UnitItems)
			{
				if (itemsByName.TryGetValue(itemDto.UnitTypeName, out var existingItem))
				{
					// Update existing item
					existingItem.Rate = itemDto.Rate;
					existingItem.IsEnabled = itemDto.IsEnabled;
					existingItem.UpdatedAt = KsaTime.Now;
					// EF Core will track changes automatically since item was loaded from context
				}
				else
				{
					// Add new item using repository
					var newItem = _mapper.Map<RateTypeUnitItem>(itemDto);
					newItem.RateTypeId = rateTypeId;
					await _unitOfWork.RateTypeUnitItems.AddAsync(newItem);
				}
			}
		}

		await _unitOfWork.RateTypes.UpdateAsync(entity);
		await _unitOfWork.SaveChangesAsync();
		return await Map(entity.Id);
	}

		public async Task<ZaaerRateTypeResponseDto?> GetByIdAsync(int rateTypeId)
		{
			var entity = await _context.RateTypes.FirstOrDefaultAsync(r => r.Id == rateTypeId);
			return entity == null ? null : await Map(entity.Id);
		}

		public async Task<IEnumerable<ZaaerRateTypeResponseDto>> GetAllByHotelIdAsync(int hotelId)
		{
			var list = await _context.RateTypes
				.Where(r => r.HotelId == hotelId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();

			var result = new List<ZaaerRateTypeResponseDto>();
			foreach (var rateType in list)
			{
				result.Add(await Map(rateType.Id));
			}
			return result;
		}

		public async Task<bool> DeleteByZaaerIdAsync(int zaaerId)
		{
			// Find rate type by ZaaerId
			var entity = await _context.RateTypes
				.FirstOrDefaultAsync(r => r.ZaaerId == zaaerId);

			if (entity == null)
			{
				return false;
			}

			var rateTypeId = entity.Id;

			// Load and delete all related RateTypeUnitItems manually (no FK constraint, so manual deletion)
			var unitItems = await _context.RateTypeUnitItems
				.Where(i => i.RateTypeId == rateTypeId)
				.ToListAsync();

			// Delete all RateTypeUnitItems at once
			if (unitItems.Any())
			{
				_context.Set<RateTypeUnitItem>().RemoveRange(unitItems);
			}

			// Delete the RateType
			await _unitOfWork.RateTypes.DeleteAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		private async Task<ZaaerRateTypeResponseDto> Map(int rateTypeId)
		{
			var entity = await _context.RateTypes.FirstAsync(r => r.Id == rateTypeId);
			
			// Load unit items manually (no FK constraint, so manual loading)
			var unitItems = await _context.RateTypeUnitItems
				.Where(i => i.RateTypeId == rateTypeId)
				.OrderBy(i => i.CreatedAt)
				.ToListAsync();

			var dto = _mapper.Map<ZaaerRateTypeResponseDto>(entity);
			dto.UnitItems = unitItems.Select(_mapper.Map<ZaaerRateTypeUnitItemResponseDto>).ToList();
			return dto;
		}
	}
}
