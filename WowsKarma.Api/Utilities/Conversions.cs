using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Utilities
{
	public static class Conversions
	{
		public static AccountListingDTO ToDTO(this AccountListing accountListing) => new(accountListing.AccountId, accountListing.Nickname);

		public static Player ToDbModel(this AccountInfo accountInfo) => new()
		{
			Id = accountInfo.AccountId,
			Username = accountInfo.Nickname,
			WgAccountCreatedAt = accountInfo.CreatedAtTime,
			WgKarma = accountInfo.Statistics.Basic.Karma,
			LastBattleTime = DateTime.UnixEpoch.AddSeconds(accountInfo.Statistics.Basic.LastBattleTime)
		};
	}
}
