using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WowsKarma.Api.Data.Models.Replays;

namespace WowsKarma.Api.Data.Models;

public sealed record Post : ITimestamped
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	[Required]
	public uint PlayerId { get; init; }
	[Required]
	public Player Player { get; init; } = null!;
	[Required]
	public uint AuthorId { get; init; }
	[Required]
	public Player Author { get; init; } = null!;

	public PostFlairs Flairs { get; set; }
	public PostFlairsParsed? ParsedFlairs => Flairs.ParseFlairsEnum();

	[Required]
	public string Title { get; set; } = "";
	[Required]
	public string Content { get; set; } = "";

	public Guid? ReplayId { get; set; }
	public Replay? Replay { get; set; }

	// Computed by DB Engine (hopefully)
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; set; }

	public bool NegativeKarmaAble { get; internal set; }

	public bool ReadOnly { get; set; }
		
	public bool ModLocked { get; set; }
}