namespace WowsKarma.Common.Models.DTOs.Clans;

public record ClanProfileDTO : ClanListingDTO
{
	public string Description { get; init; } = string.Empty;

	public bool IsDisbanded { get; init; }

	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; init; }
}

public record ClanProfileFullDTO : ClanProfileDTO
{
	public virtual List<PlayerProfileDTO> Members { get; init; } = new();
	
	public DateTimeOffset MembersUpdatedAt { get; init; }
}