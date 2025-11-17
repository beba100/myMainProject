namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for returning bulk floor operation results
    /// </summary>
    public class BulkFloorResponseDto
    {
        public int BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int TotalFloors { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<FloorResponseDto> CreatedFloors { get; set; } = new List<FloorResponseDto>();
        public List<FloorResponseDto> UpdatedFloors { get; set; } = new List<FloorResponseDto>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
