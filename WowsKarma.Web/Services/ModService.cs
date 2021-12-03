using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Common.Utilities;

namespace WowsKarma.Web.Services
{
	public class ModService : HttpServiceBase
	{
		private const string RequestUri = "mod/action";

		public ModService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : base(httpClientFactory, null, httpContextAccessor) { }

		public async Task<IEnumerable<PostModActionDTO>> GetPostModActionsAsync(Guid postId)
		{
			HttpRequestMessage request = new(HttpMethod.Get, $"{RequestUri}/list{BuildQuery(("postId", postId.ToString()))}");
			HttpResponseMessage response = await Client.SendAsync(request);
			response.EnsureSuccessStatusCode();

			return response.StatusCode is not System.Net.HttpStatusCode.NoContent
				? await response.Content.ReadFromJsonAsync<PostModActionDTO[]>(Utilities.JsonSerializerOptions)
				: null;
		}

		public async Task DeletePostAsync(Guid postId, string reason)
		{

			HttpRequestMessage request = new(HttpMethod.Post, RequestUri);
			request.Content = JsonContent.Create(new PostModActionDTO()
				{
					ActionType = ModActionType.Deletion,
					PostId = postId,
					Reason = reason
				}, null, Utilities.JsonSerializerOptions);

			HttpResponseMessage response = await Client.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}
