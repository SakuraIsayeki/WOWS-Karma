using System.Drawing;
using Mapster;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Public;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Vortex;
using WowsKarma.Common;
using ClanInfo = Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Clans.ClanInfo;

namespace WowsKarma.Api.Utilities
{
	public static class Conversions
	{
		public static void ConfigureMapping()
		{
			TypeAdapterConfig<PlayerPostDTO, Post>
				.NewConfig()
				.Ignore(dest => dest.Author)
				.Ignore(dest => dest.Player);

			TypeAdapterConfig<PostModActionDTO, PostModAction>
				.NewConfig()
				.Ignore(dest => dest.Post)
				.Ignore(dest => dest.Mod);

			TypeAdapterConfig<ClanInfo, Clan>
				.NewConfig()
				.IgnoreNullValues(true)
				.Map(dest => dest.LeagueColor, src => (uint)ColorTranslator.FromHtml(src.Color).ToArgb())
				.Ignore(dest => dest.CreatedAt)
				.Ignore(dest => dest.UpdatedAt);

			TypeAdapterConfig<Player, PlayerClanProfileDTO>
				.NewConfig()
				.IgnoreNullValues(true)
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.ClanMemberRole, src => src.ClanMember.Role)
				.Map(dest => dest.JoinedClanAt, src => src.ClanMember.JoinedAt)
				.Map(dest => dest.Clan, src => src.ClanMember.Clan);

			TypeAdapterConfig<ClanMember, PlayerClanProfileDTO>
				.NewConfig()
				.IgnoreNullValues(true)
				.Map(dest => dest, src => src.Player)
				.Map(dest => dest.Id, src => src.PlayerId);

			TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig().MapWith(x => x.UtcDateTime);
			TypeAdapterConfig<DateTime, DateTimeOffset>.NewConfig().MapWith(x => new(x));
			
			TypeAdapterConfig<DateTime, DateOnly>.NewConfig().MapWith(x => DateOnly.FromDateTime(x));
			TypeAdapterConfig<DateOnly?, DateTime>.NewConfig().MapWith(x => x == null ? DateTime.UnixEpoch : x.Value.ToDateTime(TimeOnly.MinValue));
		}

		public static AccountListingDTO ToDTO(this AccountListing accountListing) => new(accountListing.AccountId, accountListing.Nickname);

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

		public static Player[] ToDbModel(this VortexAccountInfo[] accountInfos) => Array.ConvertAll(accountInfos, ToDbModel);

		public static int ToInt(this PostFlairs input) => (int)input;
	}
}
