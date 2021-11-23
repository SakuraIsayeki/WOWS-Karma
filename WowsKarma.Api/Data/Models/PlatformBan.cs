using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WowsKarma.Api.Data.Models;


public record PlatformBan : ITimestamped
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	[Required]
	public uint UserId { get; init; }
	public virtual Player User { get; init; }

	[Required]
	public uint ModId { get; init; }
	public virtual Player Mod { get; init; }

	[Required]
	public string Reason { get; set; }

	public DateTime? BannedUntil { get; set; }

	public bool Reverted { get; set; }

	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; set; }
}
