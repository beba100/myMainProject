namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for refund statistics
    /// </summary>
    public class RefundStatisticsDto
    {
        public int TotalRefunds { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal AverageRefundAmount { get; set; }
        public decimal MaxRefundAmount { get; set; }
        public decimal MinRefundAmount { get; set; }
        public Dictionary<string, int> RefundsByHotel { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RefundsByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> TotalRefundAmountByHotel { get; set; } = new Dictionary<string, decimal>();
    }
}
