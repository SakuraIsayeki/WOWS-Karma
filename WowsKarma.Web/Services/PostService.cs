using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Services
{
	public class PostService
	{
		private readonly IHttpClientFactory httpClientFactory;
		private readonly PlayerService playerService;


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
	}
}
