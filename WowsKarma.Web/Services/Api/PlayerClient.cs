using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Services.Api;

public class PlayerClient : ApiClientBase
{
	public const string playerEndpointCategory = "player";
	public const string profileEndpointCategory = "profile";



	public PlayerClient(HttpClient httpClient, IHttpContextAccessor contextAccessor) : base(httpClient, contextAccessor) { }

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

		if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.NoContent)
		{
			return null;
		}

		await EnsureSuccessfulResponseAsync(response);
		PlayerProfileDTO player = await response.Content.ReadFromJsonAsync<PlayerProfileDTO>(SerializerOptions);
			
		return player;
	}
}