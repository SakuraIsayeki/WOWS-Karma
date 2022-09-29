using WowsKarma.Common;

#nullable enable
namespace WowsKarma.Api.Utilities;


/// <summary>
/// Provides web link utilities.
/// </summary>
public static class Links
{
	/// <summary>
	/// Gets a player's profile link on the web app.
	/// </summary>
	/// <param name="player">The player.</param>
	/// <returns>The player's profile link</returns>
	public static string GetPlayerProfileLink(this Player player) => $"{Startup.ApiRegion.GetRegionWebDomain()}player/{player.Id},{player.Username}";
	
	/// <inheritdoc cref="GetPlayerProfileLink(WowsKarma.Api.Data.Models.Player)"/>
	public static string GetPlayerProfileLink(this AccountListingDTO player) => $"{Startup.ApiRegion.GetRegionWebDomain()}player/{player.Id},{player.Username}";
	
	/// <inheritdoc cref="GetPlayerProfileLink(WowsKarma.Api.Data.Models.Player)"/>
	public static string GetPlayerProfileLink(this PlayerProfileDTO player) => $"{Startup.ApiRegion.GetRegionWebDomain()}player/{player.Id},{player.Username}";
	
	/// <summary>
	/// Gets a player post's link on the web app.
	/// </summary>
	/// <param name="post">The post.</param>
	/// <returns>The post's link</returns>
	public static string GetPostLink(this Post post) => $"{Startup.ApiRegion.GetRegionWebDomain()}posts/view/{post.Id}";
	
	/// <inheritdoc cref="GetPostLink(WowsKarma.Api.Data.Models.Post)"/>
	public static string GetPostLink(this PlayerPostDTO post) => $"{Startup.ApiRegion.GetRegionWebDomain()}posts/view/{post.Id}";
}