using DSharpPlus;
using DSharpPlus.Entities;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Discord;

public abstract class WebhookService
{
	protected DiscordWebhookClient Client { get; private init; }

	private readonly Uri _webhookUserAvatarLink;
	private readonly string _apiRegionString;

	protected WebhookService(IConfiguration configuration)
	{
		Client = new();

		_apiRegionString = Startup.ApiRegion.ToRegionString();
		_webhookUserAvatarLink = new(configuration["Discord:WebhookAvatarPath"] ?? throw new InvalidOperationException("Missing Discord:WebhookAvatarPath in configuration."));
	}

	protected DiscordWebhookBuilder GetCurrentRegionWebhookBuilder() => new()
	{
		AvatarUrl = _webhookUserAvatarLink.AbsoluteUri,
		Username = $"WOWS Karma ({_apiRegionString})"
	};

	protected DiscordEmbedBuilder.EmbedFooter GetDefaultFooter() => new()
	{
		IconUrl = _webhookUserAvatarLink.AbsoluteUri,
		Text = $"WOWS Karma ({_apiRegionString}) v{Startup.DisplayVersion} - Powered by Nodsoft Systems"
	};
}