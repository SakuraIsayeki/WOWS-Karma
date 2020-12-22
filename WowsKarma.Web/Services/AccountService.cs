using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Services
{
	public class AccountService
	{
		private readonly IHttpClientFactory httpClientFactory;

		public AccountService(IHttpClientFactory clientFactory)
		{
			httpClientFactory = clientFactory;
		}

		public async Task<IEnumerable<AccountListingDTO>> SearchPlayersAsync(string search)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"Account/Search/{search}");
			using HttpResponseMessage response = await httpClientFactory.CreateClient().SendAsync(request);

			if (response.IsSuccessStatusCode)
			{
				return await Utilities.DeserializeFromHttpResponseAsync<IEnumerable<AccountListingDTO>>(response);
			}
			
			return null;
		}

		public async Task<PlayerProfileDTO> FetchPlayerProfileAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"Account/{id}");
			using HttpResponseMessage response = await httpClientFactory.CreateClient().SendAsync(request);

			if (response.IsSuccessStatusCode)
			{
				PlayerProfileDTO player = await Utilities.DeserializeFromHttpResponseAsync<PlayerProfileDTO>(response);
				return player;
			}

			return null;
		}
	}
}
