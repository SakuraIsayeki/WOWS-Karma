using Nodsoft.Wargaming.Api.Common;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Authentication.Wargaming;

public sealed class WargamingAuthClientFactory
{
	private readonly IHttpClientFactory _httpClientFactory;

	public WargamingAuthClientFactory(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public HttpClient GetClient(Region region) => _httpClientFactory.CreateClient($"wargaming-auth-{region.ToRegionString()}");
}