using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nodsoft.WowsReplaysUnpack.Core.Models;

namespace WowsKarma.Api.Data.Models.Replays;


public sealed record Replay
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	[Required]
	public Guid PostId { get; init; }
	public Post Post { get; init; } = null!;

	public string BlobName { get; set; } = "";
	
	public bool MinimapRendered { get; set; }


	/*
	 * Replay content (stored JSON)
	 */

	[Column(TypeName = "jsonb")]
	public ArenaInfo ArenaInfo { get; set; } = null!;

	[Column(TypeName = "jsonb")]
	public IEnumerable<ReplayPlayer> Players { get; set; } = [];

	[Column(TypeName = "jsonb")]
	public IEnumerable<ReplayChatMessage> ChatMessages { get; set; } = [];
}
