using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace WowsKarma.Api.Data.Models;

public record Clan : ITimestamped
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public string Tag { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;
	
	public uint LeagueColor { get; set; }
	
	public bool IsDisbanded { get; set; }

	public virtual List<ClanMember> Members { get; set; } = new();
	
	public Instant CreatedAt { get; init; }
	public Instant UpdatedAt { get; set; }
	public Instant MembersUpdatedAt { get; set; }
}