namespace WowsKarma.Common.Models.DTOs;

public record PlayerProfileDTO
{
	public uint Id { get; init; }

	public string Username { get; init; } = string.Empty;
	
	public PlayerClanProfileDTO? Clan { get; init; }

	public int GameKarma { get; init; }
	public int SiteKarma { get; init; }

	public bool WgHidden { get; init; }
	public bool OptedOut { get; init; }

	public int RatingPerformance { get; init; }
	public int RatingTeamplay { get; init; }
	public int RatingCourtesy { get; init; }

	public DateTimeOffset WgAccountCreatedAt { get; init; }
	public DateTimeOffset LastBattleTime { get; init; }
	public DateTimeOffset OptOutChanged { get; init; }

	public bool NegativeKarmaAble { get; init; }
	public bool PostsBanned { get; init; }
}
