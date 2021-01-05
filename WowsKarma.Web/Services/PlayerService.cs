using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Services
{
	public class PlayerService
	{
		private readonly IHttpClientFactory httpClientFactory;

		public PlayerService(IHttpClientFactory clientFactory)
		{
			httpClientFactory = clientFactory;
		}

		public async Task<IEnumerable<AccountListingDTO>> SearchPlayersAsync(string search)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"Account/Search/{search}");
			using HttpResponseMessage response = await httpClientFactory.CreateClient().SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return await Utilities.DeserializeFromHttpResponseAsync<IEnumerable<AccountListingDTO>>(response);
			}
			
			return null;
		}

		public async Task<PlayerProfileDTO> FetchPlayerProfileAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"Account/{id}");
			using HttpResponseMessage response = await httpClientFactory.CreateClient().SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				PlayerProfileDTO player = await Utilities.DeserializeFromHttpResponseAsync<PlayerProfileDTO>(response);
				return new(player) { Id = id };
			}

			return null;
		}
	}
}