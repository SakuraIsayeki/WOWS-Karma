using Mapster;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Public;
using Nodsoft.Wargaming.Api.Common.Data.Responses.Wows.Vortex;

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
