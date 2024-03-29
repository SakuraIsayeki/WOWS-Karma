﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WowsKarma.Api.Data.Models;

public sealed record Player : ITimestamped
{
	internal const int NegativeKarmaAbilityThreshold = -20;


	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public string Username { get; set; } = "";

	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset UpdatedAt { get; set; }
	
	public ClanMember? ClanMember { get; set; }
	
	public bool WgHidden { get; set; }

	public int SiteKarma { get; set; }
	public int GameKarma { get; set; }

	public int PerformanceRating { get; set; }
	public int TeamplayRating { get; set; }
	public int CourtesyRating { get; set; }

	public DateTimeOffset WgAccountCreatedAt { get; init; }
	public DateTimeOffset LastBattleTime { get; set; }

	public List<Post> PostsReceived { get; init; } = [];
	public List<Post> PostsSent { get; init; } = [];

	public List<PlatformBan> PlatformBans { get; init; } = [];

	public bool NegativeKarmaAble => (SiteKarma + GameKarma) > NegativeKarmaAbilityThreshold;
	public bool PostsBanned { get; set; }
	public bool OptedOut { get; set; }
	public DateTimeOffset? OptOutChanged { get; set; }


	public bool IsBanned()
		=> PostsBanned
		|| PlatformBans?.Any(pb => !pb.Reverted && (pb.BannedUntil is null || pb.BannedUntil > DateTimeOffset.Now)) is true;



	/*
	 * Mapping
	 */

	public static implicit operator PlayerProfileDTO(Player value) => value is null ? null : new()
	{
		Id = value.Id,
		Username = value.Username,
		WgAccountCreatedAt = value.WgAccountCreatedAt,
		WgHidden = value.WgHidden,
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
