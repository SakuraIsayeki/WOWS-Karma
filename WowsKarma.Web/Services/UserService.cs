using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Web.Utilities;

namespace WowsKarma.Web.Services
{
	public class UserService : HttpServiceBase
	{
		public const string authEndpointCategory = "auth";
		public const string profileEndpointCategory = "profile";

		public UserService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor) : base(httpClientFactory, null, contextAccessor) { }

		public async Task<UserProfileFlagsDTO> GetUserProfileFlagsAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{profileEndpointCategory}/{id}");
			using HttpResponseMessage response = await Client.SendAsync(request);

			response.EnsureSuccessStatusCode();
			return await DeserializeFromHttpResponseAsync<UserProfileFlagsDTO>(response);
		}

		public async Task SetUserProfileFlagsAsync(UserProfileFlagsDTO flags)
		{
			using HttpRequestMessage request = new(HttpMethod.Put, profileEndpointCategory);
			request.Content = JsonContent.Create(flags, new("application/json"), JsonSerializerOptions);

			using HttpResponseMessage response = await Client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task RefreshSeedTokenAsync()
		{
			using HttpRequestMessage request = new(HttpMethod.Post, $"{authEndpointCategory}/renew-seed");

			using HttpResponseMessage response = await Client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}