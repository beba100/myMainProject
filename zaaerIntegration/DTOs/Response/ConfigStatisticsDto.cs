namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for config statistics
    /// </summary>
    public class ConfigStatisticsDto
    {
        public int TotalConfigs { get; set; }
        public int ConfigsWithVat { get; set; }
        public int ConfigsWithLodgingTax { get; set; }
        public int ConfigsWithCompanyName { get; set; }
        public int ConfigsWithZatcaIntegration { get; set; }
        public decimal AverageVatPercent { get; set; }
        public decimal AverageLodgingTax { get; set; }
        public Dictionary<string, int> ConfigCountByCurrency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ConfigCountByZatcaEnvironment { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> TotalVatByHotel { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> TotalLodgingTaxByHotel { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> ConfigCountByHotel { get; set; } = new Dictionary<string, int>();
    }
}
