﻿using System.Drawing;
using System.Linq.Expressions;
using Hangfire.Annotations;
using Mapster;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Public;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Vortex;
using WowsKarma.Common.Models.DTOs.Clans;
using ClanInfo = Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Clans.ClanInfo;

namespace WowsKarma.Api.Utilities;

public static class Conversions
{
	[UsedImplicitly]
	public static void ConfigureMapping()
	{
		TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileWithDebugInfo();

		TypeAdapterConfig<PlayerPostDTO, Post>
			.NewConfig()
			.IgnoreNullValues(true)
			.Ignore(dest => dest.Author)
			.Ignore(dest => dest.Player)
			.Map(
				dest => dest.CustomerSupportTicketId, 
				src => src.SupportTicketStatus.TicketId, 
				srcCond => srcCond.SupportTicketStatus.TicketId != null
			);
			
		TypeAdapterConfig<Post, PlayerPostDTO>
			.NewConfig()
			.IgnoreNullValues(true)
			.Map(
				static dest => dest.ReplayState, 
				static src => src.Replay == null 
					? ReplayState.None 
					: src.Replay.MinimapRendered
                		? ReplayState.Ready
						: ReplayState.Processing
			)
			.Map(dest => dest.Author.Clan, src => src.Author.ClanMember.Clan)
			.Map(dest => dest.Player.Clan, src => src.Player.ClanMember.Clan)
			.Map(dest => dest.SupportTicketStatus, src => new PlayerPostDTO.CustomerSupportStatus
			{
				HasTicket = src.CustomerSupportTicketId != null,
				TicketId = src.CustomerSupportTicketId
			});
		
		TypeAdapterConfig<PostModActionDTO, PostModAction>
			.NewConfig()
			.Ignore(dest => dest.Post)
			.Ignore(dest => dest.Mod);

		TypeAdapterConfig<ClanInfo, Clan>
			.NewConfig()
			.IgnoreNullValues(true)
			.Map(dest => dest.LeagueColor, src => (uint)ColorTranslator.FromHtml(src.Color).ToArgb())
			.Map(dest => dest.CreatedAt, src => src.CreatedAt.ToUniversalTime())
			.Ignore(dest => dest.UpdatedAt);

		TypeAdapterConfig<Player, PlayerProfileDTO>
			.NewConfig()
			.Map(dest => dest.Clan, src => src.ClanMember)
			.Map(dest => dest.RatingPerformance, src => src.PerformanceRating)
			.Map(dest => dest.RatingTeamplay, src => src.TeamplayRating)
			.Map(dest => dest.RatingCourtesy, src => src.CourtesyRating);

		TypeAdapterConfig<ClanMember, PlayerClanProfileDTO>
			.NewConfig()
			.Map(dest => dest.ClanInfo, src => src.Clan)
			.Map(dest => dest.JoinedClanAt, src => src.JoinedAt)
			.Map(dest => dest.ClanMemberRole, src => src.Role);
			
		TypeAdapterConfig<ClanMember, PlayerProfileDTO>
			.NewConfig()
			.IgnoreNullValues(true)
			.Unflattening(true)
			.Map(dest => dest, src => src.Player)
			.Map(dest => dest.Clan, src => src)
			.Map(dest => dest.RatingPerformance, src => src.Player.PerformanceRating)
			.Map(dest => dest.RatingTeamplay, src => src.Player.TeamplayRating)
			.Map(dest => dest.RatingCourtesy, src => src.Player.CourtesyRating);

		TypeAdapterConfig<Clan, ClanProfileFullDTO>
			.NewConfig()
			.IgnoreNullValues(true)
			.Map(dest => dest.Members, src => src.Members)
			.Fork(fork => 
				fork.ForType<ClanMember, PlayerClanProfileDTO>()
					.Ignore(dest => dest.ClanInfo));

			
		TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig().MapWith(x => x.UtcDateTime);
		TypeAdapterConfig<DateTime, DateTimeOffset>.NewConfig().MapWith(x => new(x));
			
		TypeAdapterConfig<DateTime, DateOnly>.NewConfig().MapWith(x => DateOnly.FromDateTime(x));
		TypeAdapterConfig<DateOnly?, DateTime>.NewConfig().MapWith(x => x == null ? DateTime.UnixEpoch : x.Value.ToDateTime(TimeOnly.MinValue));
		
		TypeAdapterConfig<DateTimeOffset, DateOnly>.NewConfig().MapWith(x => DateOnly.FromDateTime(x.UtcDateTime));
		TypeAdapterConfig<DateOnly?, DateTimeOffset>.NewConfig().MapWith(x => x == null ? DateTimeOffset.UnixEpoch : new(x.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
	}

	[Pure]
	public static AccountListingDTO ToDto(this AccountListing accountListing) => new(accountListing.AccountId, accountListing.Nickname);

	public static Player ToDbModel(this VortexAccountInfo accountInfo)
	{
		Player player = new()
		{
			Id = accountInfo.AccountId,
			Username = accountInfo.Nickname,
			WgHidden = accountInfo.HiddenProfile
		};

		return player.WgHidden
			? player
			: player with
			{
				WgAccountCreatedAt = accountInfo.CreatedAtTime,
				GameKarma = accountInfo.Statistics.Basic?.Karma ?? 0,
				LastBattleTime = DateTime.UnixEpoch.AddSeconds(accountInfo.Statistics.Basic!.LastBattleTime)
			};
	}
}