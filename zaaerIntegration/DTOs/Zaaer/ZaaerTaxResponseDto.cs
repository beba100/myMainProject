namespace zaaerIntegration.DTOs.Zaaer
{
	/// <summary>
	/// DTO for tax record response via Zaaer integration
	/// </summary>
	public class ZaaerTaxResponseDto
	{
		public int Id { get; set; }
		public int? ZaaerId { get; set; }
		public int? TaxId { get; set; }
		public int HotelId { get; set; }
		public string TaxName { get; set; } = string.Empty;
		public string TaxType { get; set; } = string.Empty;
		public decimal TaxRate { get; set; }
		public string? Method { get; set; }
		public bool Enabled { get; set; }
		public string? TaxCode { get; set; }
		public string? ApplyOn { get; set; }
		public string? Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}

