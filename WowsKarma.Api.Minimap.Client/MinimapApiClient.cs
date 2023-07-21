using System.Net.Http.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WowsKarma.Api.Minimap.Client;

#nullable enable

/// <summary>
/// HTTP client for the Minimap API.
/// </summary>
[PublicAPI]
public sealed class MinimapApiClient
{
	private readonly HttpClient _client;
	private readonly IOptions<MinimapApiClientOptions> _options;
	private JsonWebToken? _accessToken;
	private JsonWebToken? _refreshToken;
	
	public bool HasCredentials => _options.Value is { Username: not (null or ""), Password: not (null or "") };
    public bool IsAuthenticated => _accessToken?.ValidTo > DateTime.UtcNow;
	public bool CanRefreshToken => _refreshToken?.ValidTo > DateTime.UtcNow;
	
	public MinimapApiClient(HttpClient client, IOptions<MinimapApiClientOptions> options)
	{
		_client = client;
		_options = options;
	}

	/// <summary>
	/// Renders the game minimap for the specified replay.
	/// </summary>
	/// <param name="replay">The replay to render the minimap for, in a blob.</param>
	/// <param name="ReplayId">(optional) The ID of the replay to render the minimap for.</param>
	/// <param name="targetedPlayerId">(optional) The ID of a player to highlight/target on the minimap.</param>
	/// <returns>The rendered minimap MP4 video, in a blob.</returns> 
	public async ValueTask<byte[]> RenderReplayMinimapAsync(byte[] replay, string? ReplayId = null, uint? targetedPlayerId = null)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, "replay/minimap")
		{
			Content = new MultipartFormDataContent
			{
				{ new ByteArrayContent(replay), "replay", "replay.wowsreplay" },
				{ new StringContent(ReplayId ?? ""), "replayId" },
				{ new StringContent(targetedPlayerId?.ToString() ?? ""), "targetedPlayerId" }
			}
		};
		
		using HttpResponseMessage response = await _client.SendAsync(request);
		
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadAsByteArrayAsync();
		}

		throw new HttpRequestException($"The request failed with Status Code: {response.StatusCode} {response.ReasonPhrase}.", null, response.StatusCode)
		{
			Data =
			{
				{ "Content", await response.Content.ReadAsStringAsync() },
				{ "Headers", response.Headers }
			}
		};
	}
	
	#region Authentication

	/// <summary>
	/// Adds an authentication header to the request.
	/// </summary>
	/// <exception cref="InvalidOperationException">The client is not authenticated.</exception>
	internal HttpRequestMessage AddAuthorizationHeader(HttpRequestMessage request)
	{
		request.Headers.Authorization = new("Bearer", _accessToken?.EncodedToken ?? throw new InvalidOperationException("The client is not authenticated."));
		return request;
	}
	
	/// <summary>
	/// Logs in to the Minimap API using the configured username and password.
	/// </summary>
	/// <exception cref="HttpRequestException">The request failed.</exception>
	internal async ValueTask LoginAsync()
	{
		// Login using OAuth2 password grant flow
		using HttpResponseMessage response = await _client.PostAsync("token", new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "grant_type", "password" },
			{ "username", _options.Value.Username },
			{ "password", _options.Value.Password }
		}));
		
		// Deserialize the response and store the access token
		if (await response.Content.ReadFromJsonAsync<MinimapLoginResponse>() is { } loginResponse)
		{
			_accessToken = new(loginResponse.AccessToken);
			_refreshToken = new(loginResponse.RefreshToken);
		}
		else
		{
			throw new HttpRequestException($"The request failed with Status Code: {response.StatusCode} {response.ReasonPhrase}.", null, response.StatusCode)
			{
				Data =
				{
					{ "Content", await response.Content.ReadAsStringAsync() },
					{ "Headers", response.Headers }
				}
			};
		}
	}
	
	/// <summary>
	/// Refreshes the access token using the refresh token.
	/// </summary>
	/// <exception cref="HttpRequestException">The request failed.</exception>
	/// <exception cref="InvalidOperationException">The client is not authenticated.</exception>
	internal async ValueTask RefreshTokenAsync()
	{
		// Throw if the client is not authenticated
		if (!CanRefreshToken)
		{
			throw new InvalidOperationException("The client is not authenticated.");
		}
		
		// Refresh the access token using OAuth2 refresh token grant flow
		using HttpResponseMessage response = await _client.PostAsync("refresh_token", new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "refresh_token",
			["refresh_token"] = _refreshToken!.EncodedToken
		}));
		
		// Throw if the request failed
		response.EnsureSuccessStatusCode();
		
		// Deserialize the response
		if (await response.Content.ReadFromJsonAsync<MinimapLoginResponse>() is not { } content)
		{
			throw new InvalidOperationException("The response did not contain a refresh token.");
		}
		
		// Store the new access token
		_accessToken = new(content.AccessToken ?? throw new InvalidOperationException("The response did not contain an access token."));
		
		// Store the new refresh token
		_refreshToken = new(content.RefreshToken ?? throw new InvalidOperationException("The response did not contain a refresh token."));
		
		// Throw if the refresh token is invalid
		if (!CanRefreshToken)
		{
			throw new InvalidOperationException("The refresh token is invalid.");
		}
	}

	#endregion
	

}