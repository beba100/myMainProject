using AutoMapper;
using FinanceLedgerAPI.Models;
using Microsoft.EntityFrameworkCore;
using zaaerIntegration.Data;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
	public interface IZaaerZatcaDetailsService
	{
		Task<ZaaerZatcaDetailsResponseDto> CreateAsync(ZaaerCreateZatcaDetailsDto dto);
		Task<ZaaerZatcaDetailsResponseDto?> UpdateAsync(int detailsId, ZaaerUpdateZatcaDetailsDto dto);
		Task<IEnumerable<ZaaerZatcaDetailsResponseDto>> GetAllByHotelIdAsync(int hotelId);
		Task<ZaaerZatcaDetailsResponseDto?> GetByIdAsync(int detailsId);
	}

	public class ZaaerZatcaDetailsService : IZaaerZatcaDetailsService
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public ZaaerZatcaDetailsService(ApplicationDbContext db, IMapper mapper)
		{
			_db = db; _mapper = mapper;
		}

		public async Task<ZaaerZatcaDetailsResponseDto> CreateAsync(ZaaerCreateZatcaDetailsDto dto)
		{
			var entity = _mapper.Map<ZatcaDetails>(dto);
			_db.ZatcaDetails.Add(entity);
			await _db.SaveChangesAsync();
			return _mapper.Map<ZaaerZatcaDetailsResponseDto>(entity);
		}

		public async Task<ZaaerZatcaDetailsResponseDto?> UpdateAsync(int detailsId, ZaaerUpdateZatcaDetailsDto dto)
		{
			var entity = await _db.ZatcaDetails.FirstOrDefaultAsync(z => z.DetailsId == detailsId);
			if (entity == null) return null;
			if (dto.HotelId.HasValue) entity.HotelId = dto.HotelId.Value;
			if (dto.CompanyName != null) entity.CompanyName = dto.CompanyName;
			if (dto.TaxNumber != null) entity.TaxNumber = dto.TaxNumber;
			if (dto.GroupTaxId != null) entity.GroupTaxId = dto.GroupTaxId;
			if (dto.CorporateRegistrationNumber != null) entity.CorporateRegistrationNumber = dto.CorporateRegistrationNumber;
			if (dto.Environment != null) entity.Environment = dto.Environment;
			if (dto.Otp != null) entity.Otp = dto.Otp;
			if (dto.Address != null) entity.Address = dto.Address;
			if (dto.StreetName != null) entity.StreetName = dto.StreetName;
			if (dto.BuildingNumber != null) entity.BuildingNumber = dto.BuildingNumber;
			if (dto.PlotIdentification != null) entity.PlotIdentification = dto.PlotIdentification;
			if (dto.CitySubdivisionName != null) entity.CitySubdivisionName = dto.CitySubdivisionName;
			if (dto.City != null) entity.City = dto.City;
			if (dto.PostalZone != null) entity.PostalZone = dto.PostalZone;
			if (dto.CountrySubEntity != null) entity.CountrySubEntity = dto.CountrySubEntity;
			if (dto.CompanyRegistrationName != null) entity.CompanyRegistrationName = dto.CompanyRegistrationName;
			entity.UpdatedAt = KsaTime.Now;
			await _db.SaveChangesAsync();
			return _mapper.Map<ZaaerZatcaDetailsResponseDto>(entity);
		}

		public async Task<IEnumerable<ZaaerZatcaDetailsResponseDto>> GetAllByHotelIdAsync(int hotelId)
		{
			var list = await _db.ZatcaDetails.Where(z => z.HotelId == hotelId).ToListAsync();
			return list.Select(_mapper.Map<ZaaerZatcaDetailsResponseDto>);
		}

		public async Task<ZaaerZatcaDetailsResponseDto?> GetByIdAsync(int detailsId)
		{
			var entity = await _db.ZatcaDetails.FirstOrDefaultAsync(z => z.DetailsId == detailsId);
			return entity == null ? null : _mapper.Map<ZaaerZatcaDetailsResponseDto>(entity);
		}
	}
}


