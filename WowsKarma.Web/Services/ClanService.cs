using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Common.Models.DTOs.Clans;

namespace WowsKarma.Web.Services;

public class ClanService : HttpServiceBase
{
	public ClanService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor) : base(httpClientFactory, null, contextAccessor) { }

	public async Task<IEnumerable<ClanListingDTO>> SearchClansAsync(string search)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"clan/search/{search}");
		using HttpResponseMessage response = await Client.SendAsync(request);

		if (response.StatusCode is HttpStatusCode.OK)
		{
			return await response.Content.ReadFromJsonAsync<IEnumerable<ClanListingDTO>>(SerializerOptions);
		}
			
		return null;
	}

	public async Task<ClanProfileFullDTO> FetchClanProfileFullAsync(uint id)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"clan/{id}?includeMembers=true");
		using HttpResponseMessage response = await Client.SendAsync(request);

		if (response.StatusCode is HttpStatusCode.NotFound)
		{
			return null;
		}

		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ClanProfileFullDTO>(SerializerOptions);
	}
}