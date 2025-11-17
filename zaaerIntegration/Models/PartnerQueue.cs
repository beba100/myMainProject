using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zaaerIntegration.Models
{
	/// <summary>
	/// Queue table to store incoming partner requests before processing
	/// </summary>
	[Table("partner_request_queue")]
	public class PartnerQueue
	{
		[Key]
		[Column("queue_id")]
		public int QueueId { get; set; }

		[Column("request_ref")]
		[MaxLength(64)]
		[Required]
		public string RequestRef { get; set; } = string.Empty;

		[Column("partner")]
		[MaxLength(50)]
		[Required]
		public string Partner { get; set; } = string.Empty;

		[Column("operation")]
		[MaxLength(200)]
		[Required]
		public string Operation { get; set; } = string.Empty;

		[Column("payload_json", TypeName = "nvarchar(max)")]
		public string? PayloadJson { get; set; }

		[Column("operation_key")]
		[MaxLength(150)]
		public string? OperationKey { get; set; }

		[Column("target_id")]
		public int? TargetId { get; set; }

		[Column("payload_type")]
		[MaxLength(200)]
		public string? PayloadType { get; set; }

		[Column("status")]
		[MaxLength(50)]
		public string Status { get; set; } = "Queued";

		[Column("attempts")]
		public int Attempts { get; set; }

		[Column("last_error")] 
		public string? LastError { get; set; }

		[Column("next_attempt_at")] 
		public DateTime? NextAttemptAt { get; set; }

		[Column("created_at")] 
		public DateTime CreatedAt { get; set; } = KsaTime.Now;

		[Column("updated_at")] 
		public DateTime? UpdatedAt { get; set; }

		[Column("hotel_id")] 
		public int? HotelId { get; set; }
	}
}


