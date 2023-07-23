using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
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
	public static IServiceCollection AddMinimapApiClient(this IServiceCollection services, IConfigurationSection configurationSection)
	{
		services.AddHttpClient<MinimapApiClient>()
			.ConfigureHttpClient(static (services, client) =>
			{
				IOptions<MinimapApiClientOptions> options = services.GetRequiredService<IOptions<MinimapApiClientOptions>>();
				client.BaseAddress = new(options.Value.BaseUrl);
				client.Timeout = TimeSpan.FromSeconds(300);
			});

		services.Configure<MinimapApiClientOptions>(configurationSection);

		return services;
	}
}