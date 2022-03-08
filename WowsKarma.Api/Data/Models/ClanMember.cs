using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Api.Data.Models;

public record ClanMember
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; init; }
	
	public uint PlayerId { get; init; }
	public virtual Player Player { get; init; }

	public uint ClanId { get; init; }
	public virtual Clan Clan { get; init; }

	public DateTime JoinedAt { get; init; }
	public DateTime? LeftAt { get; set; }
	
	public ClanRole Role { get; set; }
}