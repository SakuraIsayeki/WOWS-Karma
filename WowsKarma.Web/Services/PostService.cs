using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Web.Utilities;


namespace WowsKarma.Web.Services
{
	public class PostService
	{
		private readonly HttpClient client;
		private readonly IHttpContextAccessor contextAccessor;
		public const string EndpointCategory = "post";

		public PostService(IHttpClientFactory clientfactory, IHttpContextAccessor contextAccessor)
		{
			client = clientfactory.CreateClient();
			this.contextAccessor = contextAccessor;
		}

		~PostService()
		{
			client.Dispose();
		}


		public async Task<PlayerPostDTO> FetchPostAsync(Guid id)
		{
			using HttpResponseMessage response = await client.GetAsync($"{EndpointCategory}/{id}");

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return await response.Content.ReadFromJsonAsync<PlayerPostDTO>(Utilities.JsonSerializerOptions);
			}
			else
			{
				return null;
			}
		}

		public async Task<IEnumerable<PlayerPostDTO>> FetchReceivedPostsAsync(uint id, uint fetchLast)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{EndpointCategory}/{id}/received");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return new List<PlayerPostDTO>(await DeserializeFromHttpResponseAsync<PlayerPostDTO[]>(response)).OrderByDescending(p => p.PostedAt);
			}
			else if (response.StatusCode is HttpStatusCode.NoContent)
			{
				return Enumerable.Empty<PlayerPostDTO>();
			}

			return null;
		}

		public async Task<IEnumerable<PlayerPostDTO>> FetchLatestPostsAsync(int count)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{EndpointCategory}/latest?count={count}");
			using HttpResponseMessage response = await client.SendAsync(request);

			response.EnsureSuccessStatusCode();
			return await DeserializeFromHttpResponseAsync<PlayerPostDTO[]>(response);
		}


		public async Task<IEnumerable<PlayerPostDTO>> FetchSentPostsAsync(uint id, uint fetchLast)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{EndpointCategory}/{id}/sent");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return new List<PlayerPostDTO>(await DeserializeFromHttpResponseAsync<PlayerPostDTO[]>(response)).OrderByDescending(p => p.PostedAt);
			}
			else if (response.StatusCode is HttpStatusCode.NoContent)
			{
				return Enumerable.Empty<PlayerPostDTO>();
			}

			return null;
		}

		public async Task SubmitNewPostAsync(PlayerPostDTO post)
		{
			using HttpRequestMessage request = new(HttpMethod.Post, $"{EndpointCategory}");
			request.Content = JsonContent.Create(post, new("application/json"), JsonSerializerOptions);
			request.Headers.Authorization = GenerateAuthenticationHeader(contextAccessor.HttpContext);

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task EditPostAsync(PlayerPostDTO post)
		{
			using HttpRequestMessage request = new(HttpMethod.Put, $"{EndpointCategory}");
			request.Content = JsonContent.Create(post, new("application/json"), JsonSerializerOptions);
			request.Headers.Authorization = GenerateAuthenticationHeader(contextAccessor.HttpContext);

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task DeletePostAsync(Guid postId)
		{
			using HttpRequestMessage request = new(HttpMethod.Delete, $"{EndpointCategory}/{postId}");
			request.Headers.Authorization = GenerateAuthenticationHeader(contextAccessor.HttpContext);

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}
