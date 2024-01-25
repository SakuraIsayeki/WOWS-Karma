using System.ComponentModel.DataAnnotations.Schema;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Api.Data.Models;

public sealed record ClanMember : IComparable<ClanMember>, IComparable<ClanRole>
{
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint PlayerId { get; init; }
	public Player Player { get; init; } = null!;

	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint ClanId { get; init; }
	public Clan Clan { get; init; } = null!;

	public DateOnly JoinedAt { get; init; }
	public DateOnly? LeftAt { get; set; }
	
	public ClanRole Role { get; set; }

	public bool Equals(ClanMember? other) => other is not null && other.ClanId == ClanId && other.PlayerId == PlayerId;

	public int CompareTo(ClanMember? other) => CompareTo(other?.Role ?? ClanRole.Unknown);

	// Alex thinks it's terrible.
	public int CompareTo(ClanRole other) => Role == other 
		? 0
		: (Role, other, Role > other) switch
		{
			(not ClanRole.Unknown, not ClanRole.Unknown, true) or (_, ClanRole.Unknown, _)  => 1,
			(not ClanRole.Unknown, not ClanRole.Unknown, false) or (ClanRole.Unknown, _, _) => -1
		};
}