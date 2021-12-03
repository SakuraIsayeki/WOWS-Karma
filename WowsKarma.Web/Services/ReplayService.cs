using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Shared;
using static AspNet.Security.OpenId.OpenIdAuthenticationConstants;
using static WowsKarma.Web.Utilities;


namespace WowsKarma.Web.Services;

public class ReplayService : HttpServiceBase
{
	public const string EndpointCategory = "replay";
	public const long MaxReplayFileSize = 5242880;

	public ReplayService(IHttpClientFactory clientfactory, IHttpContextAccessor contextAccessor) : base(clientfactory, null, contextAccessor) { }

	public async Task SubmitNewReplayAsync(Guid postId, IBrowserFile browserFile, CancellationToken ct)
	{
		using MultipartFormDataContent form = new();
		using StreamContent fileContent = new(browserFile.OpenReadStream(MaxReplayFileSize, ct));
		fileContent.Headers.ContentDisposition = new("form-data")
		{
			Name = "replay",
			FileName = browserFile.Name
		};

		form.Add(fileContent, "replay", browserFile.Name);

		using HttpRequestMessage request = new(HttpMethod.Post, $"{EndpointCategory}/{postId}");
		request.Content = form;


		using HttpResponseMessage response = await Client.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();
	}
}
