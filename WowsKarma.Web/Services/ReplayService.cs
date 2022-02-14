using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;



namespace WowsKarma.Web.Services;

public class ReplayService : HttpServiceBase
{
	public const string EndpointCategory = "replay";
	public const long MaxReplayFileSize = 5242880;

	public ReplayService(IHttpClientFactory clientfactory, IHttpContextAccessor contextAccessor) : base(clientfactory, null, contextAccessor) { }

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
