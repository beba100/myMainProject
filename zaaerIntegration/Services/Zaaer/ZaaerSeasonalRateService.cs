using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
	public interface IZaaerSeasonalRateService
	{
		Task<ZaaerSeasonalRateResponseDto> CreateAsync(ZaaerCreateSeasonalRateDto dto);
		Task<ZaaerSeasonalRateResponseDto?> UpdateAsync(int seasonId, ZaaerUpdateSeasonalRateDto dto);
		Task<ZaaerSeasonalRateResponseDto?> GetByIdAsync(int seasonId);
		Task<IEnumerable<ZaaerSeasonalRateResponseDto>> GetAllByHotelIdAsync(int hotelId);
	}

	public class ZaaerSeasonalRateService : IZaaerSeasonalRateService
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public ZaaerSeasonalRateService(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public async Task<ZaaerSeasonalRateResponseDto> CreateAsync(ZaaerCreateSeasonalRateDto dto)
		{
			var query = _db.SeasonalRates
				.Include(s => s.Items)
				.Where(s => s.HotelId == dto.HotelId);

			SeasonalRate? existing = null;
			if (dto.ZaaerId.HasValue)
			{
				existing = await query.FirstOrDefaultAsync(s => s.ZaaerId == dto.ZaaerId.Value);
			}
			if (existing == null)
			{
				existing = await query.FirstOrDefaultAsync(s =>
					s.ZaaerId == null &&
					s.Title == dto.Title &&
					s.DateFrom == dto.DateFrom &&
					s.DateTo == dto.DateTo);
			}

			var incomingItems = dto.Items
				.GroupBy(i => i.RoomTypeId)
				.Select(g => g.Last())
				.ToList();

			if (existing != null)
			{
				existing.Title = dto.Title;
				existing.Description = dto.Description;
				existing.DateFrom = dto.DateFrom;
				existing.DateTo = dto.DateTo;
				existing.HotelId = dto.HotelId;
				existing.ZaaerId = dto.ZaaerId;
				existing.UpdatedAt = KsaTime.Now;

				var itemsByRoomType = existing.Items.ToDictionary(i => i.RoomTypeId);
				foreach (var itemDto in incomingItems)
				{
					if (itemsByRoomType.TryGetValue(itemDto.RoomTypeId, out var existingItem))
					{
						existingItem.DailyRateLowWeekdays = itemDto.DailyRateLowWeekdays;
						existingItem.DailyRateHighWeekdays = itemDto.DailyRateHighWeekdays;
						existingItem.OtaRateLowWeekdays = itemDto.OtaRateLowWeekdays;
						existingItem.OtaRateHighWeekdays = itemDto.OtaRateHighWeekdays;
					}
					else
					{
						var newItem = _mapper.Map<SeasonalRateItem>(itemDto);
						newItem.SeasonId = existing.SeasonId;
						_db.SeasonalRateItems.Add(newItem);
					}
				}

				await _db.SaveChangesAsync();
				return await Map(existing.SeasonId);
			}

			var entity = _mapper.Map<SeasonalRate>(dto);
			_db.SeasonalRates.Add(entity);
			await _db.SaveChangesAsync();

			foreach (var itemDto in incomingItems)
			{
				var item = _mapper.Map<SeasonalRateItem>(itemDto);
				item.SeasonId = entity.SeasonId;
				_db.SeasonalRateItems.Add(item);
			}
			await _db.SaveChangesAsync();

			return await Map(entity.SeasonId);
		}

		public async Task<ZaaerSeasonalRateResponseDto?> UpdateAsync(int seasonId, ZaaerUpdateSeasonalRateDto dto)
		{
			var entity = await _db.SeasonalRates.Include(s => s.Items).FirstOrDefaultAsync(s => s.SeasonId == seasonId);
			if (entity == null) return null;

			if (dto.HotelId.HasValue) entity.HotelId = dto.HotelId.Value;
			if (!string.IsNullOrWhiteSpace(dto.Title)) entity.Title = dto.Title!;
			if (dto.Description != null) entity.Description = dto.Description;
			if (dto.DateFrom.HasValue) entity.DateFrom = dto.DateFrom.Value;
			if (dto.DateTo.HasValue) entity.DateTo = dto.DateTo.Value;
			entity.UpdatedAt = KsaTime.Now;

			// Upsert items by RoomTypeId
			var itemsByRoomType = entity.Items.ToDictionary(i => i.RoomTypeId);
			foreach (var itemDto in dto.Items)
			{
				if (itemsByRoomType.TryGetValue(itemDto.RoomTypeId, out var existing))
				{
					existing.DailyRateLowWeekdays = itemDto.DailyRateLowWeekdays;
					existing.DailyRateHighWeekdays = itemDto.DailyRateHighWeekdays;
					existing.OtaRateLowWeekdays = itemDto.OtaRateLowWeekdays;
					existing.OtaRateHighWeekdays = itemDto.OtaRateHighWeekdays;
				}
				else
				{
					var newItem = _mapper.Map<SeasonalRateItem>(itemDto);
					newItem.SeasonId = entity.SeasonId;
					_db.SeasonalRateItems.Add(newItem);
				}
			}

			await _db.SaveChangesAsync();
			return await Map(entity.SeasonId);
		}

		public async Task<ZaaerSeasonalRateResponseDto?> GetByIdAsync(int seasonId)
		{
			var entity = await _db.SeasonalRates.Include(s => s.Items).FirstOrDefaultAsync(s => s.SeasonId == seasonId);
			return entity == null ? null : await Map(entity.SeasonId);
		}

		public async Task<IEnumerable<ZaaerSeasonalRateResponseDto>> GetAllByHotelIdAsync(int hotelId)
		{
			var list = await _db.SeasonalRates
				.Include(s => s.Items)
				.Where(s => s.HotelId == hotelId)
				.OrderByDescending(s => s.DateFrom)
				.ToListAsync();

			return list.Select(s =>
			{
				var dto = _mapper.Map<ZaaerSeasonalRateResponseDto>(s);
				dto.Items = s.Items.Select(_mapper.Map<ZaaerSeasonalRateItemResponseDto>).ToList();
				return dto;
			});
		}

		private async Task<ZaaerSeasonalRateResponseDto> Map(int seasonId)
		{
			var entity = await _db.SeasonalRates.Include(s => s.Items).FirstAsync(s => s.SeasonId == seasonId);
			var dto = _mapper.Map<ZaaerSeasonalRateResponseDto>(entity);
			dto.Items = entity.Items.Select(_mapper.Map<ZaaerSeasonalRateItemResponseDto>).ToList();
			return dto;
		}
	}
}


