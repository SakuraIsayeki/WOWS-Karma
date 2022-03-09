using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Common.Models.DTOs;

public record PlayerClanProfileDTO : PlayerProfileDTO
{
	public ClanListingDTO Clan { get; init; }
	
	public ClanRole ClanMemberRole { get; init; }
	
	public LocalDate JoinedClanAt { get; init; }
}