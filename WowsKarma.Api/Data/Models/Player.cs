using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace WowsKarma.Api.Data.Models;

public record Player : ITimestamped
{
	internal const int NegativeKarmaAbilityThreshold = -20;


	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public string Username { get; set; }

	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; set; }

	public bool WgHidden { get; set; }

	public int SiteKarma { get; set; }
	public int GameKarma { get; set; }

	public int PerformanceRating { get; set; }
	public int TeamplayRating { get; set; }
	public int CourtesyRating { get; set; }

	public DateTime WgAccountCreatedAt { get; init; }
	public DateTime LastBattleTime { get; set; }

	public virtual List<Post> PostsReceived { get; init; }
	public virtual List<Post> PostsSent { get; init; }

	public virtual List<PlatformBan> PlatformBans { get; init; }

	public bool NegativeKarmaAble => (SiteKarma + GameKarma) > NegativeKarmaAbilityThreshold;
	public bool PostsBanned { get; set; }
	public bool OptedOut { get; set; }
	public DateTime OptOutChanged { get; set; }


	public bool IsBanned()
		=> PostsBanned
		|| PlatformBans?.Where(pb => !pb.Reverted && (pb.BannedUntil is null || pb.BannedUntil > DateTime.UtcNow)).Any() is true;



	/*
	 * Mapping
	 */

	public static implicit operator PlayerProfileDTO(Player value) => new()
	{
		Id = value.Id,
		Username = value.Username,
		WgAccountCreatedAt = value.WgAccountCreatedAt,
		Hidden = value.WgHidden,
		OptedOut = value.OptedOut,
		OptOutChanged = value.OptOutChanged,
		GameKarma = value.GameKarma,
		SiteKarma = value.SiteKarma,
		RatingPerformance = value.PerformanceRating,
		RatingTeamplay = value.TeamplayRating,
		RatingCourtesy = value.CourtesyRating,
		LastBattleTime = value.LastBattleTime
	};

	public static Player MapFromApi(Player source, Player mod)
	{
		source.Username = mod.Username;
		source.GameKarma = mod.GameKarma;
		source.LastBattleTime = mod.LastBattleTime;
		source.WgHidden = mod.WgHidden;

		return source;
	}
}
