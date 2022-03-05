using DSharpPlus;
using DSharpPlus.Entities;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Discord
{
	public abstract class WebhookService
	{
		protected DiscordWebhookClient Client { get; private init; }

		private readonly Uri webhookUserAvatarLink;
		private readonly string apiRegionString;

		public WebhookService(IConfiguration configuration)
		{
			Client = new();

			apiRegionString = Startup.ApiRegion.ToRegionString();
			webhookUserAvatarLink = new(configuration["Discord:WebhookAvatarPath"]);
		}

		protected DiscordWebhookBuilder GetCurrentRegionWebhookBuilder() => new()
		{
			AvatarUrl = webhookUserAvatarLink.AbsoluteUri,
			Username = $"WOWS Karma ({apiRegionString})"
		};

		protected DiscordEmbedBuilder.EmbedFooter GetDefaultFooter() => new()
		{
			IconUrl = webhookUserAvatarLink.AbsoluteUri,
			Text = $"WOWS Karma ({apiRegionString}) v{Startup.DisplayVersion} - Powered by Nodsoft Systems"
		};
	}
}
