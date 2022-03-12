using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Api.Data.Models;

public record ClanMember : IComparable<ClanMember>, IComparable<ClanRole>
{
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint PlayerId { get; init; }
	public virtual Player Player { get; init; }

	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint ClanId { get; init; }
	public virtual Clan Clan { get; init; }

	public DateOnly JoinedAt { get; init; }
	public DateOnly? LeftAt { get; set; }
	
	public ClanRole Role { get; set; }

	public virtual bool Equals(ClanMember other) => other is not null && other.ClanId == ClanId && other.PlayerId == PlayerId;

	public int CompareTo(ClanMember other) => CompareTo(other?.Role ?? ClanRole.Unknown);

	// Alex thinks it's terrible.
	public int CompareTo(ClanRole other) => Role == other 
		? 0
		: (Role, other, Role > other) switch
		{
			(not ClanRole.Unknown, not ClanRole.Unknown, true) or (_, ClanRole.Unknown, _)  => 1,
			(not ClanRole.Unknown, not ClanRole.Unknown, false) or (ClanRole.Unknown, _, _) => -1
		};
}