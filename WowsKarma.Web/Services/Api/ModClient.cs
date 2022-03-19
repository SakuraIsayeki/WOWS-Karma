using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using static WowsKarma.Common.Utilities;

namespace WowsKarma.Web.Services.Api;

public class ModClient : ApiClientBase
{
	private const string RequestUri = "mod/action";

	public ModClient(HttpClient httpClient, IHttpContextAccessor contextAccessor) : base(httpClient, contextAccessor) { }

	public async Task<IEnumerable<PostModActionDTO>> GetPostModActionsAsync(Guid postId)
	{
		HttpRequestMessage request = new(HttpMethod.Get, $"{RequestUri}/list{BuildQuery(("postId", postId.ToString()))}");
		HttpResponseMessage response = await Client.SendAsync(request);
		await EnsureSuccessfulResponseAsync(response);

		return response.StatusCode is not System.Net.HttpStatusCode.NoContent
			? await response.Content.ReadFromJsonAsync<PostModActionDTO[]>(SerializerOptions)
			: null;
	}

	public async Task DeletePostAsync(Guid postId, string reason)
	{

		HttpRequestMessage request = new(HttpMethod.Post, RequestUri);
		request.Content = JsonContent.Create(new PostModActionDTO
		{
			ActionType = ModActionType.Deletion,
			PostId = postId,
			Reason = reason
		}, null, SerializerOptions);

		HttpResponseMessage response = await Client.SendAsync(request);
		await EnsureSuccessfulResponseAsync(response);
	}
}