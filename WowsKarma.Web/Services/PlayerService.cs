using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
				return await response.Content.ReadFromJsonAsync<IEnumerable<AccountListingDTO>>(SerializerOptions);
			}
			
			return null;
		}

		public async Task<PlayerProfileDTO> FetchPlayerProfileAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{playerEndpointCategory}/{id}?includeClanInfo=true");
			using HttpResponseMessage response = await Client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.NotFound)
			{
				return null;
			}

			response.EnsureSuccessStatusCode();
			PlayerProfileDTO player = await response.Content.ReadFromJsonAsync<PlayerProfileDTO>(SerializerOptions);
			
			return player;
		}
	}
}