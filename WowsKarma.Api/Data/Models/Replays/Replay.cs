using Nodsoft.WowsReplaysUnpack.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WowsKarma.Api.Data.Models.Replays;


public record Replay
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	[Required]
	public Guid PostId { get; init; }
	public virtual Post Post { get; init; }

	public string BlobName { get; set; }


	/*
	 * Replay content (stored JSON)
	 */

	[Column(TypeName = "jsonb")]
	public virtual ArenaInfo ArenaInfo { get; set; }

	[Column(TypeName = "jsonb")]
	public virtual IEnumerable<ReplayPlayer> Players { get; set; }

	[Column(TypeName = "jsonb")]
	public virtual IEnumerable<ReplayChatMessage> ChatMessages { get; set; }
}
