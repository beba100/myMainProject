using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zaaerIntegration.Models
{
	/// <summary>
	/// Log table to keep immutable history of processed partner requests
	/// </summary>
	[Table("partner_request_log")]
	public class PartnerRequestLog
	{
		[Key]
		[Column("log_id")]
		public int LogId { get; set; }

		[Column("request_ref")]
		[MaxLength(64)]
		public string? RequestRef { get; set; }

		[Column("partner")]
		[MaxLength(50)]
		public string? Partner { get; set; }

		[Column("operation")]
		[MaxLength(200)]
		public string? Operation { get; set; }

		[Column("status")]
		[MaxLength(50)]
		public string? Status { get; set; }

		[Column("message")]
		public string? Message { get; set; }

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("hotel_id")]
		public int? HotelId { get; set; }
	}
}


