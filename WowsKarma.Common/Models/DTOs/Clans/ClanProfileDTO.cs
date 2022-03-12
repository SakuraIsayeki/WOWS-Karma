namespace WowsKarma.Common.Models.DTOs.Clans;

public record ClanProfileDTO : ClanListingDTO
{
	public string Description { get; set; } = string.Empty;

	public bool IsDisbanded { get; set; }

	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; set; }
}

public record ClanProfileFullDTO : ClanProfileDTO
{
	public virtual IEnumerable<PlayerClanProfileDTO>? Members { get; set; }
	
	public DateTime MembersUpdatedAt { get; set; }
}