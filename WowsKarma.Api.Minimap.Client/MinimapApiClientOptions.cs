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
	public string BaseUrl { get; set; } = "https://minimap.api.wows-karma.com/";

	/// <summary>
	/// The username to log in with.
	/// </summary>
	public string Login { get; set; } = null!;
	
	/// <summary>
	/// The password to log in with.
	/// </summary>
	public string Password { get; set; } = null!;
}