using Microsoft.AspNetCore.Authorization;

namespace WowsKarma.Api.Infrastructure.Authorization;

/// <summary>
/// Provides an authorization requirement to evaluate a user's platform bans.
/// </summary>
public class PlatformBanRequirement : IAuthorizationRequirement
{
	/// <summary>
	/// Whether the user should be banned from the current platform.
	/// </summary>
	public bool IsBanned { get; init; }
	

	/// <param name="isBanned">Whether the user should be banned from the current platform.</param>
	public PlatformBanRequirement(bool isBanned = false)
	{
		IsBanned = isBanned;
	}
}