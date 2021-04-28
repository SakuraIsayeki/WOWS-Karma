using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Models;

namespace WowsKarma.Web.Services
{
	public class PostService
	{
		private readonly HttpClient client;
		public const string EndpointCategory = "Post";

		private static readonly JsonSerializerOptions serializerOptions = new() 
		{ 
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
		};


		public PostService(IHttpClientFactory clientfactory)
		{
			client = clientfactory.CreateClient();
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
				return await response.Content.ReadFromJsonAsync<PlayerPostDTO>(serializerOptions);
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
				return new List<PlayerPostDTO>(await Utilities.DeserializeFromHttpResponseAsync<PlayerPostDTO[]>(response)).OrderByDescending(p => p.PostedAt);
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
			return await Utilities.DeserializeFromHttpResponseAsync<PlayerPostDTO[]>(response);
		}


		public async Task<IEnumerable<PlayerPostDTO>> FetchSentPostsAsync(uint id, uint fetchLast)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{EndpointCategory}/{id}/sent");
			using HttpResponseMessage response = await client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return new List<PlayerPostDTO>(await Utilities.DeserializeFromHttpResponseAsync<PlayerPostDTO[]>(response)).OrderByDescending(p => p.PostedAt);
			}
			else if (response.StatusCode is HttpStatusCode.NoContent)
			{
				return Enumerable.Empty<PlayerPostDTO>();
			}

			return null;
		}

		public async Task SubmitNewPostAsync(uint authorId, PlayerPostDTO post)
		{
			using HttpRequestMessage request = new(HttpMethod.Post, $"{EndpointCategory}/{authorId}");
			string json = JsonSerializer.Serialize(post, serializerOptions);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task EditPostAsync(uint authorId, PlayerPostDTO post)
		{
			using HttpRequestMessage request = new(HttpMethod.Put, $"{EndpointCategory}/{authorId}");
			string json = JsonSerializer.Serialize(post, serializerOptions);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task DeletePostAsync(Guid postId)
		{
			using HttpRequestMessage request = new(HttpMethod.Delete, $"{EndpointCategory}/{postId}");
			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}
