using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Models;

namespace WowsKarma.Web.Services
{
	public class PostService
	{
		private readonly IHttpClientFactory httpClientFactory;


		public PostService(IHttpClientFactory clientfactory)
		{
			httpClientFactory = clientfactory;
		}

		public async Task<IEnumerable<PlayerPostDTO>> FetchReceivedPostsAsync(uint id, uint fetchLast)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"Post/{id}/received");
			using HttpResponseMessage response = await httpClientFactory.CreateClient().SendAsync(request);

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
			using HttpRequestMessage request = new(HttpMethod.Post, $"Post/{authorId}");
			string json = JsonSerializer.Serialize(post, new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");

			using HttpResponseMessage response = await httpClientFactory.CreateClient().SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}
