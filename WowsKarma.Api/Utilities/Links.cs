using WowsKarma.Api.Data.Models;
using WowsKarma.Common;



namespace WowsKarma.Api.Utilities
{
	public static class Links
	{
		public static string GetPlayerProfileLink(this Player player) => $"{Startup.ApiRegion.GetRegionWebDomain()}player/{player.Id},{player.Username}";
		public static string GetPostProfileLink(this Post post) => $"{Startup.ApiRegion.GetRegionWebDomain()}post/view/{post.Id}";
	}
}
