using JetBrains.Annotations;

namespace WowsKarma.Api.Minimap.Client;

/// <summary>
/// Represents the response from the Minimap API when logging in through OAuth2 password grant flow.
/// </summary>
[UsedImplicitly]
internal sealed record MinimapLoginResponse
{
	/// <summary>
	/// The access token to use for future requests.
	/// </summary>
	public string AccessToken { get; init; } = null!;

	/// <summary>
	/// The refresh token to use for future requests.
	/// </summary>
	public string RefreshToken { get; init; } = null!;
	
	/// <summary>
	/// The type of the access token.
	/// </summary>
	public string TokenType { get; init; } = null!;
}