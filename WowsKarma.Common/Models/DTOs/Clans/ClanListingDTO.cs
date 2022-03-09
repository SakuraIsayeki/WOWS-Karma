namespace WowsKarma.Common.Models.DTOs;

public record ClanListingDTO
{
	public uint Id { get; init; }

	public string Tag { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;

	public uint LeagueColor { get; set; }
}