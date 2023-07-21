using JetBrains.Annotations;

namespace WowsKarma.Api.Minimap.Client;

/// <summary>
/// Options for the Minimap API client.
/// </summary>
[PublicAPI]
public sealed record MinimapApiClientOptions
{
	/// <summary>
	/// The base URL of the Minimap API.
	/// </summary>
	public Uri BaseUrl { get; init; } = new("https://minimap.api.wows-karma.com/");

	/// <summary>
	/// The username to log in with.
	/// </summary>
	public string Username { get; init; } = null!;
	
	/// <summary>
	/// The password to log in with.
	/// </summary>
	public string Password { get; init; } = null!;
}