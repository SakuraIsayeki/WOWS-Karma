using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WowsKarma.Api.Data.Models;


public sealed record PlatformBan : ITimestamped
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	[Required]
	public uint UserId { get; init; }
	public Player User { get; init; }

	[Required]
	public uint ModId { get; init; }
	public Player Mod { get; init; }

	[Required]
	public string Reason { get; set; }

	public DateTimeOffset? BannedUntil { get; set; }

	public bool Reverted { get; set; }

	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; set; }
}
