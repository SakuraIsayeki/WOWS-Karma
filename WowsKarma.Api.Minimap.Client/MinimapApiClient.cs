using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using WowsKarma.Api.Minimap.Client.Infrastructure.Json;

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
	private readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
	};
	
	public bool HasCredentials => _options.Value is { Login: not (null or ""), Password: not (null or "") };
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
	/// <param name="replayId">(optional) The ID of the replay to render the minimap for.</param>
	/// <param name="targetedPlayerId">(optional) The ID of a player to highlight/target on the minimap.</param>
	/// <returns>The rendered minimap MP4 video, in a blob.</returns> 
	public async ValueTask<byte[]> RenderReplayMinimapAsync(byte[] replay, string? replayId = null, uint? targetedPlayerId = null, CancellationToken ct = default)
	{
		// Build the query string
		var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

		if (replayId is not (null or ""))
		{
			query.Add("replay_id", replayId);
		}
		
		if (targetedPlayerId is not null)
		{
			query.Add("target_player_id", targetedPlayerId.ToString());
		}


		using HttpRequestMessage request = await AttemptAuthenticationAsync(new(HttpMethod.Post, $"render?{query}"));

		// Open a stream to the replay blob
		await using MemoryStream replayStream = new(replay, false);
		
		request.Content = new MultipartFormDataContent
		{
			{ new StreamContent(replayStream), "file", "replay.wowsreplay" }
		};

		using HttpResponseMessage response = await _client.SendAsync(request, ct);
		
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadAsByteArrayAsync(ct);
		}

		throw new HttpRequestException($"The request failed with Status Code: {response.StatusCode} {response.ReasonPhrase}.", null, response.StatusCode)
		{
			Data =
			{
				{ "Content", await response.Content.ReadAsStringAsync(ct) },
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
			{ "username", _options.Value.Login },
			{ "password", _options.Value.Password }
		}));
		
		// Deserialize the response and store the access token
		if (await response.Content.ReadFromJsonAsync<MinimapLoginResponse>(_jsonSerializerOptions) is { } loginResponse)
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
		if (await response.Content.ReadFromJsonAsync<MinimapLoginResponse>(_jsonSerializerOptions) is not { } content)
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
	
	internal async ValueTask<HttpRequestMessage> AttemptAuthenticationAsync(HttpRequestMessage request)
	{
		// If the access token is expired, refresh it
		if (!IsAuthenticated && CanRefreshToken)
		{
			await RefreshTokenAsync();
		}
		else if (!IsAuthenticated && HasCredentials)
		{
			await LoginAsync();
		}
		
		// Add the access token to the request
		if (IsAuthenticated)
		{
			request = AddAuthorizationHeader(request);
		}

		return request;
	}

	#endregion
	

}