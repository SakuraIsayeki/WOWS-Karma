using System.ComponentModel.DataAnnotations;


namespace WowsKarma.Api.Data.Models;


public record PlatformBan : ITimestamped
{
	[Key]
	public Guid Id { get; init; }

	[Required]
	public uint UserId { get; init; }
	public virtual Player User { get; init; }

	[Required]
	public string Reason { get; set; }

	public DateTime? BannedUntil { get; set; }

	public bool Reverted { get; set; }

	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; set; }
}
