using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using Wargaming.WebAPI.Requests;
using WowsKarma.Api.Utilities;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class WgApiFetcherService
	{
		private readonly WorldOfWarshipsHandler wowsHandler;
		private readonly VortexApiHandler vortexHandler;

		public WgApiFetcherService(WorldOfWarshipsHandler wows, VortexApiHandler vortex)
		{
			wowsHandler = wows;
			vortexHandler = vortex;
		}

		public async Task<IEnumerable<AccountListingDTO>> ListAccountsAsync(string search)
		{
			IEnumerable<AccountListing> result = await wowsHandler.ListPlayersAsync(search);

			if (result.Count() is 0)
			{
				return null;
			}

			return result.Select(listing => listing.ToDTO());
		}

		public async Task<PlayerProfileDTO> FetchAcccountAsync(uint id) => (await vortexHandler.FetchAccountAsync(id))?.ToDTO();
	}
}
