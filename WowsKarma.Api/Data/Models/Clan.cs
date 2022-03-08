using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

	public List<ClanMember> Members { get; init; } = new();
	
	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; set; }
}