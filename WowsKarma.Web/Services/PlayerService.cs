using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Services
{
	public class PlayerService
	{
		private readonly HttpClient client;
		public const string endpointCategory = "Player";

		public PlayerService(IHttpClientFactory httpClientFactory)
		{
			client = httpClientFactory.CreateClient();
		}

		~PlayerService()
		{
			client.Dispose();
		}

		public async Task<IEnumerable<AccountListingDTO>> SearchPlayersAsync(string search)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{endpointCategory}/Search/{search}");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return await Utilities.DeserializeFromHttpResponseAsync<IEnumerable<AccountListingDTO>>(response);
			}
			
			return null;
		}

		public async Task<PlayerProfileDTO> FetchPlayerProfileAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{endpointCategory}/{id}");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				PlayerProfileDTO player = await Utilities.DeserializeFromHttpResponseAsync<PlayerProfileDTO>(response);
				return new(player) { Id = id };
			}

			return null;
		}

		public async Task<bool> CheckBannedPlayerAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{endpointCategory}/{id}/banned");
			using HttpResponseMessage response = await client.SendAsync(request);

			response.EnsureSuccessStatusCode();
			return await Utilities.DeserializeFromHttpResponseAsync<bool>(response);
		}
	}
}