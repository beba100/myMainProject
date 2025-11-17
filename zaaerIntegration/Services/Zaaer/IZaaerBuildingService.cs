using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Services.Zaaer
{
	public interface IZaaerBuildingService
	{
		Task<ZaaerBuildingResponseDto> CreateBuildingWithFloorsAsync(ZaaerCreateBuildingDto dto);
		Task<ZaaerBuildingResponseDto> UpdateBuildingWithFloorsAsync(ZaaerUpdateBuildingDto dto);
		Task<ZaaerBuildingResponseDto> UpdateBuildingWithFloorsSafeAsync(ZaaerUpdateBuildingDto dto);
		Task<List<ZaaerBuildingResponseDto>> GetAllBuildingsWithFloorsAsync(int hotelId);
	}
}
