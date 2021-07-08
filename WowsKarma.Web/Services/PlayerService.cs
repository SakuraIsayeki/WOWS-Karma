using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Web.Utilities;

namespace WowsKarma.Web.Services
{
	public class PlayerService : HttpServiceBase
	{
		public const string playerEndpointCategory = "player";
		public const string profileEndpointCategory = "profile";



		public PlayerService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor) : base(httpClientFactory, null, contextAccessor) { }

		public async Task<IEnumerable<AccountListingDTO>> SearchPlayersAsync(string search)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{playerEndpointCategory}/Search/{search}");
			using HttpResponseMessage response = await Client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return await DeserializeFromHttpResponseAsync<IEnumerable<AccountListingDTO>>(response);
			}
			
			return null;
		}

		public async Task<PlayerProfileDTO> FetchPlayerProfileAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{playerEndpointCategory}/{id}");
			using HttpResponseMessage response = await Client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				PlayerProfileDTO player = await DeserializeFromHttpResponseAsync<PlayerProfileDTO>(response);
				return player with { Id = id };
			}

			return null;
		}
	}
}