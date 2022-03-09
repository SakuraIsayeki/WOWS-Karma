namespace WowsKarma.Common.Models.DTOs;

public record ClanProfileDTO : ClanListingDTO
{
	public string Description { get; set; } = string.Empty;

	public bool IsDisbanded { get; set; }

	public Instant CreatedAt { get; init; }
	public Instant UpdatedAt { get; set; }
}

public record ClanProfileFullDTO : ClanProfileDTO
{
	public virtual IEnumerable<PlayerClanProfileDTO>? Members { get; set; }
}