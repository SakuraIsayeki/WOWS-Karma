using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WowsKarma.Api.Data.Models;

public sealed record Clan : ITimestamped
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public string Tag { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;
	
	public uint LeagueColor { get; set; }
	
	public bool IsDisbanded { get; set; }

	public List<ClanMember> Members { get; set; } = [];
	
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; set; }
	public DateTimeOffset MembersUpdatedAt { get; set; }
}