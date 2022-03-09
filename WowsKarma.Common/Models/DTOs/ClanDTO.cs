namespace WowsKarma.Common.Models.DTOs;

public record ClanDTO
{
	public uint Id { get; init; }

	public string Tag { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;
	
	public uint LeagueColor { get; set; }
	
	public bool IsDisbanded { get; set; }

	public Instant CreatedAt { get; init; }
	public Instant UpdatedAt { get; set; }
}

public record ClanFullDTO : ClanDTO
{
	public virtual IEnumerable<PlayerClanProfileDTO>? Members { get; set; }
}