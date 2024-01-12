using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WowsKarma.Api.Data.Models;

public sealed record Clan : ITimestamped
{
	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public string Tag { get; set; } = "";
	public string Name { get; set; } = "";

	public string Description { get; set; } = "";
	
	public uint LeagueColor { get; set; }
	
	public bool IsDisbanded { get; set; }

	public List<ClanMember> Members { get; set; } = [];
	
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; set; }
	public DateTimeOffset MembersUpdatedAt { get; set; }
}