using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows;

namespace WowsKarma.Common.Models.DTOs;

public record PlayerClanProfileDTO : PlayerProfileDTO
{
	public uint ClanId { get; init; }
	public string ClanTag { get; init; }
	public string ClanName { get; init; }
	public uint ClanColor { get; set; }
	
	public ClanRole Role { get; init; }
	public Instant JoinedAt { get; init; }
}