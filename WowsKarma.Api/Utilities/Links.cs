using WowsKarma.Common;



namespace WowsKarma.Api.Utilities
{
	public static class Links
	{
		public static string GetPlayerProfileLink(this Player player) => $"{Startup.ApiRegion.GetRegionWebDomain()}player/{player.Id},{player.Username}";
		public static string GetPostLink(this Post post) => $"{Startup.ApiRegion.GetRegionWebDomain()}posts/view/{post.Id}";
	}
}
