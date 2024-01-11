using DSharpPlus.Entities;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Discord;

public class ModActionWebhookService : WebhookService
{
	public ModActionWebhookService(IConfiguration configuration) : base(configuration)
	{
		foreach (string webhookLink in configuration.GetSection($"Discord:Webhooks:{Startup.ApiRegion.ToRegionString()}:ModActions").Get<string[]>())
		{
			Client.AddWebhookAsync(new(webhookLink)).GetAwaiter().GetResult();
		}
	}

	public async Task SendModActionWebhookAsync(PostModAction modAction)
	{
		DiscordEmbedBuilder embed = new()
		{
			Title = "**Post Locked by Mod-Action**",
			Url = modAction.Post.GetPostLink(),
			Footer = GetDefaultFooter(),
			Color = modAction.ActionType switch
			{
				ModActionType.Deletion => DiscordColor.Red,
				ModActionType.Update => DiscordColor.Yellow,
				_ => throw new ArgumentOutOfRangeException(nameof(modAction))
			}
		};

		AddModActionContent(embed, modAction);

		await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder()
			.WithContent(modAction.ActionType switch
			{
				ModActionType.Deletion => "**Single Post Deletion**",
				ModActionType.Update => "**Post Mod-Edition**",
				_ => throw new ArgumentOutOfRangeException(nameof(modAction))
			})
			.AddEmbed(embed));
	}

	public async Task SendModActionRevertedWebhookAsync(PostModAction modAction)
	{
		DiscordEmbedBuilder embed = new()
		{
			Title = $"**Post Mod-Action Reverted**",
			Url = modAction.Post.GetPostLink(),
			Footer = GetDefaultFooter(),
			Color = DiscordColor.Red
		};

		AddModActionRevertContent(embed, modAction);

		await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder()
			.WithContent("**Reverted Post Mod-Action**")
			.AddEmbed(embed));
	}

	public async Task SendPlatformBanWebhookAsync(PlatformBan ban)
	{
		DiscordEmbedBuilder embed = new()
		{
			Title = "**Account Banned from Platform**",
			Url = ban.User.GetPlayerProfileLink(),
			Footer = GetDefaultFooter(),
			Color = DiscordColor.Red
		};

		embed.AddField("Banned by", $"[{ban.Mod?.Username ?? "Unknown"}]({ban.Mod.GetPlayerProfileLink()})", true);
		embed.AddField("Reason", ban.Reason, false);

		if (ban.BannedUntil is not null)
		{
			embed.AddField("Until", $"<t:{ban.BannedUntil?.UtcDateTime.ToUnixTimestamp()}:F>");
		}

		await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder()
			.WithContent("**Account Platform Ban**")
			.AddEmbed(embed));
	}

	private static DiscordEmbedBuilder AddModActionContent(DiscordEmbedBuilder embed, PostModAction modAction)
	{
		embed.AddField("Moderated by", $"[{modAction.Mod?.Username ?? "Unknown"}]({modAction.Mod.GetPlayerProfileLink()})", true);
		embed.AddField("Post Author", $"[{modAction.Post.Author?.Username ?? "Unknown"}]({modAction.Post.Author?.GetPlayerProfileLink()})", true);
		embed.AddField("Reason", modAction.Reason, false);

		return embed;
	}

	private static DiscordEmbedBuilder AddModActionRevertContent(DiscordEmbedBuilder embed, PostModAction modAction)
	{
		embed.AddField("Mod-Action ID", modAction.Id.ToString(), true);
		return embed;
	}
}