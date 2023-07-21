using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace WowsKarma.Api.Minimap.Client;

/// <summary>
/// Delegating handler for the Minimap API client, which handles OAuth2 password grant flow authentication.
/// </summary>
public sealed class MinimapClientAuthenticationDelegatingHandler : DelegatingHandler
{
	private readonly IOptions<MinimapApiClientOptions> _options;
	private readonly MinimapApiClient _client;
	
	public MinimapClientAuthenticationDelegatingHandler(IOptions<MinimapApiClientOptions> options, MinimapApiClient client)
	{
		_options = options;
		_client = client;
	}
	
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// If the access token is expired, refresh it
		if (_client is { IsAuthenticated: false, CanRefreshToken: true })
		{
			await _client.RefreshTokenAsync();
		}
		else if (_client is { IsAuthenticated: false, HasCredentials: true })
		{
			await _client.LoginAsync();
		}
		
		// Add the access token to the request
		if (_client.IsAuthenticated)
		{
			request = _client.AddAuthorizationHeader(request);
		}
		
		return await base.SendAsync(request, cancellationToken);
	}
}