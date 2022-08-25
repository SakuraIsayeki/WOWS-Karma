using DSharpPlus.Entities;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs.Replays;


namespace WowsKarma.Api.Services.Discord;

public class PostWebhookService : WebhookService
{
	public PostWebhookService(IConfiguration configuration) : base(configuration)
	{
		foreach (string webhookLink in configuration.GetSection($"Discord:Webhooks:{Startup.ApiRegion.ToRegionString()}:Posts").Get<string[]>())
		{
			Client.AddWebhookAsync(new(webhookLink)).GetAwaiter().GetResult();
		}
	}


	public async Task SendNewPostWebhookAsync(PlayerPostDTO post)
	{
		DiscordEmbedBuilder embed = new()
		{
			Author = new() { Name = post.Author.Username, Url = post.Author.GetPlayerProfileLink() },
			Title = $"**New Post on {post.Player.Username} :** \"{post.Title}\".",
			Url = post.GetPostLink(),
			Footer = GetDefaultFooter(),
			Color = DiscordColor.Green
		};

		AddReplayStatus(embed, post.Replay);
		AddPostContent(embed, post);

		await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder().AddEmbed(embed));
	}

	public async Task SendEditedPostWebhookAsync(PlayerPostDTO post)
	{
		DiscordEmbedBuilder embed = new()
		{
			Author = new() { Name = post.Author.Username, Url = post.Author.GetPlayerProfileLink() },
			Title = $"**Edited Post on {post.Player.Username} :** \"{post.Title}\".",
			Url = post.GetPostLink(),
			Footer = GetDefaultFooter(),
			Color = new(0xffc400) // Dark Yellow
		};

		AddPostContent(embed, post);

		await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder().AddEmbed(embed));
	}

	public async Task SendDeletedPostWebhookAsync(PlayerPostDTO post)
	{
		DiscordEmbedBuilder embed = new()
		{
			Author = new() { Name = post.Author.Username, Url = post.Author.GetPlayerProfileLink() },
			Title = $"**Deleted Post on {post.Player.Username} :** \"{post.Title}\".",
			Url = post.GetPostLink(),
			Footer = GetDefaultFooter(),
			Color = DiscordColor.Red
		};

		embed.AddField("Post ID", post.Id.ToString());

		await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder().AddEmbed(embed));
	}

	private static string GetFlairValueString(bool? value) => value switch
	{
		true => "Positive",
		false => "Negative",
		null or _ => "Neutral"
	};

	private static DiscordEmbedBuilder AddReplayStatus(DiscordEmbedBuilder embed, ReplayDTO replay) => embed.AddField("Replay", replay is null
		? "*No replay provided*"
		: $"[{replay.Id}]({replay.DownloadUri})"
	);

	private static DiscordEmbedBuilder AddPostContent(DiscordEmbedBuilder embed, PlayerPostDTO post)
	{
		embed.Description = post.Content;
		PostFlairsParsed parsedFlairs = post.Flairs.ParseFlairsEnum();

		embed.AddField("Performance", GetFlairValueString(parsedFlairs?.Performance), true);
		embed.AddField("Teamplay", GetFlairValueString(parsedFlairs?.Teamplay), true);
		embed.AddField("Courtesy", GetFlairValueString(parsedFlairs?.Courtesy), true);

		return embed;
	}
}