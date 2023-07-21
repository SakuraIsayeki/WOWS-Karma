using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WowsKarma.Api.Minimap.Client;

/// <summary>
/// Extensions for the dependency injection container.
/// </summary>
[PublicAPI]
public static class MinimapApiDependencyInjectionExtensions
{
	/// <summary>
	/// Adds the Minimap API client to the dependency injection container.
	/// </summary>
	/// <param name="services">The service collection to add the client to.</param>
	/// <param name="configure">An action to configure the client options.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMinimapApiClient(this IServiceCollection services, Action<MinimapApiClientOptions> configure)
	{
		services.AddHttpClient<MinimapApiClient>()
			.ConfigureHttpClient((provider, client) =>
			{
				IOptions<MinimapApiClientOptions> options = provider.GetRequiredService<IOptions<MinimapApiClientOptions>>();
				client.BaseAddress = options.Value.BaseUrl;
			})
			.AddHttpMessageHandler<MinimapClientAuthenticationDelegatingHandler>();
		
		services.AddTransient<MinimapClientAuthenticationDelegatingHandler>();
        services.Configure(configure);
		
		return services;
	}
}