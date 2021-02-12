using System;
using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Utilities
{
	public static class Conversions
	{
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
