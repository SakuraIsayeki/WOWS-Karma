using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Utilities
{
	public static class Conversions
	{
		public static AccountListingDTO ToDTO(this AccountListing accountListing) => new(accountListing.AccountId, accountListing.Nickname);

		public static PlayerProfileDTO ToDTO(this AccountInfo accountInfo) => new()
		{
			Id = accountInfo.AccountId,
			Username = accountInfo.Nickname,
			WgAccountCreatedAt = accountInfo.CreatedAtTime,
			WgKarma = accountInfo.Statistics.Basic.Karma
		};
	}
}
