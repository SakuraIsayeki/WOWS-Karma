namespace WowsKarma.Common.Models.DTOs.Clans;

public record ClanProfileDTO : ClanListingDTO
{
	public string Description { get; init; } = string.Empty;

	public bool IsDisbanded { get; init; }

	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; init; }
}

public record ClanProfileFullDTO : ClanProfileDTO
{
	public virtual List<PlayerProfileDTO> Members { get; init; } = new();
	
	public DateTime MembersUpdatedAt { get; init; }
}