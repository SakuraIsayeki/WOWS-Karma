using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mapster;
using WowsKarma.Common;


namespace WowsKarma.Api.Data.Models;

public record Player : ITimestamped
{
	internal const int NegativeKarmaAbilityThreshold = -20;


	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public string Username { get; set; }

	public Instant CreatedAt { get; init; }
	public Instant UpdatedAt { get; set; }
	
	public virtual ClanMember ClanMember { get; set; }
	
	public bool WgHidden { get; set; }

	public int SiteKarma { get; set; }
	public int GameKarma { get; set; }

	public int PerformanceRating { get; set; }
	public int TeamplayRating { get; set; }
	public int CourtesyRating { get; set; }

	public Instant WgAccountCreatedAt { get; init; }
	public Instant LastBattleTime { get; set; }

	public virtual List<Post> PostsReceived { get; init; } = new();
	public virtual List<Post> PostsSent { get; init; } = new();

	public virtual List<PlatformBan> PlatformBans { get; init; } = new();

	public bool NegativeKarmaAble => (SiteKarma + GameKarma) > NegativeKarmaAbilityThreshold;
	public bool PostsBanned { get; set; }
	public bool OptedOut { get; set; }
	public Instant OptOutChanged { get; set; }


	public bool IsBanned()
		=> PostsBanned
		|| PlatformBans?.Where(pb => !pb.Reverted && (pb.BannedUntil is null || pb.BannedUntil > Time.Now)).Any() is true;



	/*
	 * Mapping
	 */

	public static implicit operator PlayerProfileDTO(Player value) => value is null ? null : new()
	{
		Id = value.Id,
		Username = value.Username,
		WgAccountCreatedAt = value.WgAccountCreatedAt.Adapt<DateTimeOffset>(),
		Hidden = value.WgHidden,
		OptedOut = value.OptedOut,
		OptOutChanged = value.OptOutChanged.Adapt<DateTimeOffset>(),
		GameKarma = value.GameKarma,
		SiteKarma = value.SiteKarma,
		RatingPerformance = value.PerformanceRating,
		RatingTeamplay = value.TeamplayRating,
		RatingCourtesy = value.CourtesyRating,
		LastBattleTime = value.LastBattleTime.Adapt<DateTimeOffset>()
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
