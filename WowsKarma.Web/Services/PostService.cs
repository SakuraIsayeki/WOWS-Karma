using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Web.Utilities;
using static WowsKarma.Common.Utilities;


namespace WowsKarma.Web.Services
{
	public class PostService : HttpServiceBase
	{
		public const string EndpointCategory = "post";

		public PostService(IHttpClientFactory clientfactory, IHttpContextAccessor contextAccessor) : base(clientfactory, null, contextAccessor)	{ }

		public async Task<PlayerPostDTO> FetchPostAsync(Guid id)
		{
			using HttpResponseMessage response = await Client.GetAsync($"{EndpointCategory}/{id}");

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return await response.Content.ReadFromJsonAsync<PlayerPostDTO>(SerializerOptions);
			}
			else
			{
				return null;
			}
		}

		public async Task<IEnumerable<PlayerPostDTO>> FetchReceivedPostsAsync(uint id, uint fetchLast)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{EndpointCategory}/{id}/received");
			using HttpResponseMessage response = await Client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return new List<PlayerPostDTO>(await response.Content.ReadFromJsonAsync<PlayerPostDTO[]>(SerializerOptions) ?? Array.Empty<PlayerPostDTO>())
					.OrderByDescending(p => p.CreatedAt);
			}
			else if (response.StatusCode is HttpStatusCode.NoContent)
			{
				return Enumerable.Empty<PlayerPostDTO>();
			}

			return null;
		}

		public async Task<IEnumerable<PlayerPostDTO>> FetchLatestPostsAsync(int count, bool? hasReplay = null, bool hideModActions = false)
		{
			string query = $"{EndpointCategory}/latest?count={count}";
			
			if (hasReplay.HasValue)
			{
				query += $"&hasReplay={hasReplay.Value}";
			}

			query += $"&hideModActions={hideModActions}";

			using HttpRequestMessage request = new(HttpMethod.Get, query);
			using HttpResponseMessage response = await Client.SendAsync(request);

			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<PlayerPostDTO[]>(SerializerOptions);
		}


		public async Task<IEnumerable<PlayerPostDTO>> FetchSentPostsAsync(uint id, uint fetchLast)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"{EndpointCategory}/{id}/sent");
			using HttpResponseMessage response = await Client.SendAsync(request);

			if (response.StatusCode is HttpStatusCode.OK)
			{
				return new List<PlayerPostDTO>(await response.Content.ReadFromJsonAsync<PlayerPostDTO[]>(SerializerOptions)).OrderByDescending(p => p.CreatedAt);
			}
			else if (response.StatusCode is HttpStatusCode.NoContent)
			{
				return Enumerable.Empty<PlayerPostDTO>();
			}

			return null;
		}

		public async Task<Guid> SubmitNewPostAsync(PlayerPostDTO post, IBrowserFile replayFile = null, CancellationToken ct = default)
		{
			using MultipartFormDataContent form = new();
			using StreamContent replayFileStream = replayFile is null ? null : form.AddReplayFile(replayFile, ct);
			form.Add(JsonContent.Create(post, new("application/json"), ApiSerializerOptions), "postDto");

			using HttpRequestMessage request = new(HttpMethod.Post, $"{EndpointCategory}")
			{
				Content = form
			};
			
			using HttpResponseMessage response = await Client.SendAsync(request, ct);
			response.EnsureSuccessStatusCode();

			return new((await response.Content.ReadAsStringAsync(ct)).Replace("\"", null));
		}

		public async Task EditPostAsync(PlayerPostDTO post)
		{
			using HttpRequestMessage request = new(HttpMethod.Put, $"{EndpointCategory}");
			request.Content = JsonContent.Create(post, new("application/json"), ApiSerializerOptions);

			using HttpResponseMessage response = await Client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task DeletePostAsync(Guid postId)
		{
			using HttpRequestMessage request = new(HttpMethod.Delete, $"{EndpointCategory}/{postId}");

			using HttpResponseMessage response = await Client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}
