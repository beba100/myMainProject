using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Repositories.Interfaces;

namespace zaaerIntegration.Services.Zaaer
{
	public class ZaaerBuildingService : IZaaerBuildingService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ZaaerBuildingService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<ZaaerBuildingResponseDto> CreateBuildingWithFloorsAsync(ZaaerCreateBuildingDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var building = new Building
				{
					HotelId = dto.HotelId,
					BuildingName = dto.BuildingName,
					BuildingNumber = dto.BuildingNumber ?? string.Empty,
					Address = dto.Address ?? string.Empty
				};

				await _unitOfWork.Buildings.AddAsync(building);
				await _unitOfWork.SaveChangesAsync();

				var createdFloors = new List<Floor>();
				if (dto.Floors != null && dto.Floors.Count > 0)
				{
					foreach (var f in dto.Floors)
					{
						var floor = new Floor
						{
							HotelId = dto.HotelId,
							BuildingId = building.BuildingId,
							FloorNumber = f.FloorNumber,
							FloorName = f.FloorName ?? string.Empty
						};
						var added = await _unitOfWork.Floors.AddAsync(floor);
						createdFloors.Add(added);
					}
					await _unitOfWork.SaveChangesAsync();
				}

				await _unitOfWork.CommitTransactionAsync();

				return new ZaaerBuildingResponseDto
				{
					BuildingId = building.BuildingId,
					BuildingName = building.BuildingName,
					HotelId = building.HotelId,
					Floors = createdFloors.Select(cf => new ZaaerFloorResponseItemDto
					{
						FloorId = cf.FloorId,
						FloorNumber = cf.FloorNumber,
						FloorName = cf.FloorName
					}).ToList()
				};
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<ZaaerBuildingResponseDto> UpdateBuildingWithFloorsAsync(ZaaerUpdateBuildingDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// Pre-check for floor deletion conflicts
				await ValidateFloorDeletionsAsync(dto);
				// 1. Update the building
				var existingBuilding = await _unitOfWork.Buildings.GetByIdAsync(dto.BuildingId);
				if (existingBuilding == null)
				{
					throw new KeyNotFoundException($"Building with ID {dto.BuildingId} not found");
				}

				existingBuilding.BuildingName = dto.BuildingName;
				existingBuilding.BuildingNumber = dto.BuildingNumber ?? string.Empty;
				existingBuilding.Address = dto.Address ?? string.Empty;

				_unitOfWork.Buildings.Update(existingBuilding);
				await _unitOfWork.SaveChangesAsync();

				// 2. Handle floors (update existing, add new, remove deleted)
				var existingFloors = await _unitOfWork.Floors.GetAllAsync();
				var buildingFloors = existingFloors.Where(f => f.BuildingId == dto.BuildingId).ToList();

				// Remove floors that are not in the new list (only if no apartments depend on them)
				var floorsToRemove = buildingFloors.Where(ef => !dto.Floors.Any(nf => nf.FloorId == ef.FloorId)).ToList();
				foreach (var floor in floorsToRemove)
				{
					// Check if any apartments depend on this floor
					var apartments = await _unitOfWork.Apartments.GetAllAsync();
					var dependentApartments = apartments.Where(a => a.FloorId == floor.FloorId).ToList();
					
					if (dependentApartments.Any())
					{
						// Skip deletion if apartments depend on this floor
						// Log warning or throw exception based on business requirements
						throw new InvalidOperationException($"Cannot delete floor '{floor.FloorName}' (ID: {floor.FloorId}) because it has {dependentApartments.Count} apartment(s) assigned to it. Please reassign or delete the apartments first.");
					}
					
					_unitOfWork.Floors.Delete(floor);
				}

				// Update existing floors and add new ones
				var updatedFloors = new List<Floor>();
				foreach (var floorDto in dto.Floors)
				{
					if (floorDto.FloorId.HasValue)
					{
						// Update existing floor
						var existingFloor = buildingFloors.FirstOrDefault(f => f.FloorId == floorDto.FloorId.Value);
						if (existingFloor != null)
						{
							existingFloor.FloorNumber = floorDto.FloorNumber;
							existingFloor.FloorName = floorDto.FloorName ?? string.Empty;
							_unitOfWork.Floors.Update(existingFloor);
							updatedFloors.Add(existingFloor);
						}
					}
					else
					{
						// Add new floor
						var newFloor = new Floor
						{
							HotelId = dto.HotelId,
							BuildingId = dto.BuildingId,
							FloorNumber = floorDto.FloorNumber,
							FloorName = floorDto.FloorName ?? string.Empty
						};
						var added = await _unitOfWork.Floors.AddAsync(newFloor);
						updatedFloors.Add(added);
					}
				}

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return new ZaaerBuildingResponseDto
				{
					BuildingId = existingBuilding.BuildingId,
					BuildingName = existingBuilding.BuildingName,
					HotelId = existingBuilding.HotelId,
					Floors = updatedFloors.Select(f => new ZaaerFloorResponseItemDto
					{
						FloorId = f.FloorId,
						FloorNumber = f.FloorNumber,
						FloorName = f.FloorName
					}).ToList()
				};
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<List<ZaaerBuildingResponseDto>> GetAllBuildingsWithFloorsAsync(int hotelId)
		{
			var buildings = await _unitOfWork.Buildings.GetAllAsync();
			var hotelBuildings = buildings.Where(b => b.HotelId == hotelId).ToList();

			var result = new List<ZaaerBuildingResponseDto>();
			foreach (var building in hotelBuildings)
			{
				var floors = await _unitOfWork.Floors.GetAllAsync();
				var buildingFloors = floors.Where(f => f.BuildingId == building.BuildingId).ToList();

				result.Add(new ZaaerBuildingResponseDto
				{
					BuildingId = building.BuildingId,
					BuildingName = building.BuildingName,
					HotelId = building.HotelId,
					Floors = buildingFloors.Select(f => new ZaaerFloorResponseItemDto
					{
						FloorId = f.FloorId,
						FloorNumber = f.FloorNumber,
						FloorName = f.FloorName
					}).ToList()
				});
			}

			return result;
		}

		/// <summary>
		/// Safe update method that only adds/updates floors without deleting existing ones
		/// This prevents foreign key constraint violations with apartments
		/// </summary>
		public async Task<ZaaerBuildingResponseDto> UpdateBuildingWithFloorsSafeAsync(ZaaerUpdateBuildingDto dto)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// 1. Update the building
				var existingBuilding = await _unitOfWork.Buildings.GetByIdAsync(dto.BuildingId);
				if (existingBuilding == null)
				{
					throw new KeyNotFoundException($"Building with ID {dto.BuildingId} not found");
				}

				existingBuilding.BuildingName = dto.BuildingName;
				existingBuilding.BuildingNumber = dto.BuildingNumber ?? string.Empty;
				existingBuilding.Address = dto.Address ?? string.Empty;

				_unitOfWork.Buildings.Update(existingBuilding);
				await _unitOfWork.SaveChangesAsync();

				// 2. Handle floors (only update existing and add new ones - NO DELETION)
				var existingFloors = await _unitOfWork.Floors.GetAllAsync();
				var buildingFloors = existingFloors.Where(f => f.BuildingId == dto.BuildingId).ToList();

				var updatedFloors = new List<Floor>();
				foreach (var floorDto in dto.Floors)
				{
					if (floorDto.FloorId.HasValue)
					{
						// Update existing floor
						var existingFloor = buildingFloors.FirstOrDefault(f => f.FloorId == floorDto.FloorId.Value);
						if (existingFloor != null)
						{
							existingFloor.FloorNumber = floorDto.FloorNumber;
							existingFloor.FloorName = floorDto.FloorName ?? string.Empty;
							_unitOfWork.Floors.Update(existingFloor);
							updatedFloors.Add(existingFloor);
						}
					}
					else
					{
						// Add new floor
						var newFloor = new Floor
						{
							HotelId = dto.HotelId,
							BuildingId = dto.BuildingId,
							FloorNumber = floorDto.FloorNumber,
							FloorName = floorDto.FloorName ?? string.Empty
						};
						var added = await _unitOfWork.Floors.AddAsync(newFloor);
						updatedFloors.Add(added);
					}
				}

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				// Return all floors (existing + updated + new)
				var allFloors = await _unitOfWork.Floors.GetAllAsync();
				var allBuildingFloors = allFloors.Where(f => f.BuildingId == dto.BuildingId).ToList();

				return new ZaaerBuildingResponseDto
				{
					BuildingId = existingBuilding.BuildingId,
					BuildingName = existingBuilding.BuildingName,
					HotelId = existingBuilding.HotelId,
					Floors = allBuildingFloors.Select(f => new ZaaerFloorResponseItemDto
					{
						FloorId = f.FloorId,
						FloorNumber = f.FloorNumber,
						FloorName = f.FloorName
					}).ToList()
				};
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		/// <summary>
		/// Validates that floors can be safely deleted without violating foreign key constraints
		/// </summary>
		private async Task ValidateFloorDeletionsAsync(ZaaerUpdateBuildingDto dto)
		{
			var existingFloors = await _unitOfWork.Floors.GetAllAsync();
			var buildingFloors = existingFloors.Where(f => f.BuildingId == dto.BuildingId).ToList();
			var floorsToRemove = buildingFloors.Where(ef => !dto.Floors.Any(nf => nf.FloorId == ef.FloorId)).ToList();

			if (floorsToRemove.Any())
			{
				var apartments = await _unitOfWork.Apartments.GetAllAsync();
				var conflicts = new List<string>();

				foreach (var floor in floorsToRemove)
				{
					var dependentApartments = apartments.Where(a => a.FloorId == floor.FloorId).ToList();
					if (dependentApartments.Any())
					{
						conflicts.Add($"Floor '{floor.FloorName}' (ID: {floor.FloorId}) has {dependentApartments.Count} apartment(s) assigned to it");
					}
				}

				if (conflicts.Any())
				{
					var message = "Cannot delete the following floors due to dependent apartments:\n" + string.Join("\n", conflicts);
					message += "\n\nPlease reassign or delete the apartments first, or use a different update strategy.";
					throw new InvalidOperationException(message);
				}
			}
		}
	}
}


