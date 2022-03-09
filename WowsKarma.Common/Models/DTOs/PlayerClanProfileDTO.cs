using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Common.Models.DTOs;

public record PlayerClanProfileDTO : PlayerProfileDTO
{
	public ClanDTO Clan { get; init; }
	
	public ClanRole ClanMemberRole { get; init; }
}