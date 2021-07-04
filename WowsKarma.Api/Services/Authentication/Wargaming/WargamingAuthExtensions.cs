using System.Net.Http;
using System;
using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;
using WowsKarma.Api;



namespace Microsoft.Extensions.DependencyInjection
{
	public static class WargamingAuthExtensions
	{
		public static IServiceCollection AddWargamingAuth(this IServiceCollection services)
		{
			services.AddSingleton<WargamingAuthService>();
			services.AddSingleton<WargamingAuthClientFactory>();

			services.AddHttpClient($"wargaming-auth-{Startup.ApiRegion.ToRegionString()}", c =>
			{
				c.BaseAddress = new Uri($"https://{Startup.ApiRegion.ToWargamingSubdomain()}.wargaming.net");
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			}).ConfigureHttpMessageHandlerBuilder(c => c.PrimaryHandler = new HttpClientHandler() { MaxConnectionsPerServer = 200, UseProxy = false });

			return services;
		}
	}
}
