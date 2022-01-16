namespace WowsKarma.Api.Data.Models;

public record PostReaction
{
	public Guid PostId { get; init; }
	public virtual Post Post { get; init; }

	public uint PlayerId { get; init; }
	public virtual Player Player { get; init; }
}
