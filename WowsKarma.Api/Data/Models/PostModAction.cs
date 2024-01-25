using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WowsKarma.Api.Data.Models;

/**
 *	Conversion Mapping done in <see cref="Utilities.Conversions"/>.
 **/
public sealed record PostModAction
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }

	public Post Post { get; init; } = null!;
	public Guid PostId { get; init; }

	public ModActionType ActionType { get; init; }

	public Player Mod { get; init; } = null!;
	public uint ModId { get; init; }

	public string Reason { get; set; } = "";
}
