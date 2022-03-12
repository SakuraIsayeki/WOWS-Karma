using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Common.Models.DTOs;

public record PlayerClanProfileDTO : PlayerProfileDTO
{
	public ClanListingDTO Clan { get; init; }
	
	public ClanRole ClanMemberRole { get; init; }
	
	// HACK: DateOnly / TimeOnly serialization is not supported by STJ as of now.
	// See: https://github.com/dotnet/runtime/issues/53539
	public DateTime JoinedClanAt { get; init; }
}