using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using WowsKarma.Common.Models.DTOs;


namespace WowsKarma.Web.Services.Api;

public class UserClient : ApiClientBase
{
	public const string authEndpointCategory = "auth";
	public const string profileEndpointCategory = "profile";

	public UserClient(HttpClient httpClient, IHttpContextAccessor contextAccessor) : base(httpClient, contextAccessor) { }

	public async Task<UserProfileFlagsDTO> GetUserProfileFlagsAsync(uint id)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"{profileEndpointCategory}/{id}");
		using HttpResponseMessage response = await Client.SendAsync(request);

		await EnsureSuccessfulResponseAsync(response);
		return await response.Content.ReadFromJsonAsync<UserProfileFlagsDTO>(SerializerOptions);
	}

	public async Task SetUserProfileFlagsAsync(UserProfileFlagsDTO flags)
	{
		using HttpRequestMessage request = new(HttpMethod.Put, profileEndpointCategory);
		request.Content = JsonContent.Create(flags, new("application/json"), SerializerOptions);

		using HttpResponseMessage response = await Client.SendAsync(request);
		await EnsureSuccessfulResponseAsync(response);
	}

	public async Task RefreshSeedTokenAsync()
	{
		using HttpRequestMessage request = new(HttpMethod.Post, $"{authEndpointCategory}/renew-seed");

		using HttpResponseMessage response = await Client.SendAsync(request);
		await EnsureSuccessfulResponseAsync(response);
	}

	public async Task<IEnumerable<PlatformBanDTO>> FetchUserBansAsync(uint id)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"mod/bans/{id}");
		using HttpResponseMessage response = await Client.SendAsync(request);

		if (response.IsSuccessStatusCode)
		{
			return response.StatusCode is not HttpStatusCode.NoContent
				? await response.Content.ReadFromJsonAsync<IEnumerable<PlatformBanDTO>>(SerializerOptions)
				: null;
		}

		return null;
	}
}
