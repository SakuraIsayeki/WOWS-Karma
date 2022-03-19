using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace WowsKarma.Web.Services.Api;

public class ReplayService : ApiClientBase
{
	public const string EndpointCategory = "replay";
	public const long MaxReplayFileSize = 5242880;

	public ReplayService(HttpClient httpClient, IHttpContextAccessor contextAccessor) : base(httpClient, contextAccessor) { }

	public async Task SubmitNewReplayAsync(Guid postId, IBrowserFile browserFile, CancellationToken ct)
	{
		using MultipartFormDataContent form = new();
		form.AddReplayFile(browserFile, ct);

		using HttpRequestMessage request = new(HttpMethod.Post, $"{EndpointCategory}/{postId}")
		{
			Content = form
		};


		using HttpResponseMessage response = await Client.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();
	}
}
