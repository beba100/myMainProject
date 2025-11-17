namespace zaaerIntegration.DTOs.Response
{
    /// <summary>
    /// DTO for credit note statistics
    /// </summary>
    public class CreditNoteStatisticsDto
    {
        public int TotalCreditNotes { get; set; }
        public decimal TotalCreditNoteAmount { get; set; }
        public decimal AverageCreditNoteAmount { get; set; }
        public decimal MaxCreditNoteAmount { get; set; }
        public decimal MinCreditNoteAmount { get; set; }
        public Dictionary<string, int> CreditNotesByHotel { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CreditNotesByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> TotalCreditNoteAmountByHotel { get; set; } = new Dictionary<string, decimal>();
    }
}
