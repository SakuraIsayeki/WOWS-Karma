using WowsKarma.Api.Services.Authentication.Wargaming;
using WowsKarma.Common;
using WowsKarma.Api;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class WargamingAuthExtensions
{
	public static IServiceCollection AddWargamingAuth(this IServiceCollection services)
	{
		services.AddSingleton<WargamingAuthService>();
		services.AddSingleton<WargamingAuthClientFactory>();

		services.AddHttpClient($"wargaming-auth-{Startup.ApiRegion.ToRegionString()}", c =>
		{
			c.BaseAddress = new($"https://{Startup.ApiRegion.ToWargamingSubdomain()}.wargaming.net");
			c.DefaultRequestHeaders.Add("Accept", "application/json");
		})
		.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { MaxConnectionsPerServer = 200, UseProxy = false });

		return services;
	}
}