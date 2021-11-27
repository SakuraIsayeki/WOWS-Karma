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

	public string BlobName { get; init; }

	[Column(TypeName = "jsonb")]
	public virtual ReplayArenaInfo ArenaInfo { get; init; }
}
