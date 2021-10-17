using Mapster;
using System;
using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Utilities
{
	public static class Conversions
	{
		public static void ConfigureMapping()
		{
			TypeAdapterConfig<Post, PlayerPostDTO>
				.NewConfig()
				.Map(dest => dest.AuthorUsername, src => src.Author.Username)
				.Map(dest => dest.PlayerUsername, src => src.Player.Username);

			TypeAdapterConfig<PlayerPostDTO, Post>
				.NewConfig()
				.Ignore(dest => dest.Author)
				.Ignore(dest => dest.Player);

			TypeAdapterConfig<PostModAction, PostModActionDTO>
				.NewConfig()
				.IgnoreNullValues(true)
				.Map(dest => dest.ModUsername, src => src.Mod.Username);

			TypeAdapterConfig<PostModActionDTO, PostModAction>
				.NewConfig()
				.Ignore(dest => dest.Post)
				.Ignore(dest => dest.Mod);
		}

		public static AccountListingDTO ToDTO(this AccountListing accountListing) => new(accountListing.AccountId, accountListing.Nickname);

		public static Player ToDbModel(this AccountInfo accountInfo)
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
					GameKarma = accountInfo.Statistics.Basic.Karma,
					LastBattleTime = DateTime.UnixEpoch.AddSeconds(accountInfo.Statistics.Basic.LastBattleTime)
				};
		}

		public static Player[] ToDbModel(this AccountInfo[] accountInfos) => Array.ConvertAll(accountInfos, new Converter<AccountInfo, Player>(ToDbModel));

		public static int ToInt(this PostFlairs input) => (int)input;
	}
}
