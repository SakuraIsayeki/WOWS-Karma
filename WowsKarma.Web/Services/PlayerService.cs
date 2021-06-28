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
	public class PlayerService
	{
		private readonly HttpClient client;
		private readonly IHttpContextAccessor contextAccessor;
		public const string playerEndpointCategory = "player";
		public const string profileEndpointCategory = "profile";



		public PlayerService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor)
		{
			client = httpClientFactory.CreateClient();
			this.contextAccessor = contextAccessor;
		}

		~PlayerService()
		{
			client.Dispose();
		}

		public async Task<IEnumerable<AccountListingDTO>> SearchPlayersAsync(string search)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{playerEndpointCategory}/Search/{search}");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return await DeserializeFromHttpResponseAsync<IEnumerable<AccountListingDTO>>(response);
			}
			
			return null;
		}

		public async Task<PlayerProfileDTO> FetchPlayerProfileAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{playerEndpointCategory}/{id}");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				PlayerProfileDTO player = await DeserializeFromHttpResponseAsync<PlayerProfileDTO>(response);
				return player with { Id = id };
			}

			return null;
		}

		public async Task<UserProfileFlagsDTO> GetUserProfileFlagsAsync(uint id)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{profileEndpointCategory}/{id}");
			using HttpResponseMessage response = await client.SendAsync(request);

			response.EnsureSuccessStatusCode();
			return await DeserializeFromHttpResponseAsync<UserProfileFlagsDTO>(response);
		}

		public async Task SetUserProfileFlagsAsync(UserProfileFlagsDTO flags)
		{
			using HttpRequestMessage request = new(HttpMethod.Put, profileEndpointCategory);
			request.Content = JsonContent.Create(flags, new("application/json"), JsonSerializerOptions);
			request.Headers.Authorization = GenerateAuthenticationHeader(contextAccessor.HttpContext);

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}