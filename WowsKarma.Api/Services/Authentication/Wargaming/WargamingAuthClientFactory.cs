using System.Net.Http;
using Nodsoft.Wargaming.Api.Common;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Authentication.Wargaming
{
	public class WargamingAuthClientFactory
	{
		private readonly IHttpClientFactory httpClientFactory;

		public WargamingAuthClientFactory(IHttpClientFactory httpClientFactory)
		{
			this.httpClientFactory = httpClientFactory;
		}

		public HttpClient GetClient(Region region) => httpClientFactory.CreateClient("wargaming-auth-" + region.ToRegionString());
	}
}
