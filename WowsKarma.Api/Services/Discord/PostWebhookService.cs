using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Discord
{
	public class PostWebhookService : WebhookService
	{
		public PostWebhookService(IConfiguration configuration) : base(configuration)
		{
			foreach (string webhookLink in configuration.GetSection($"Discord:Webhooks:{Startup.ApiRegion.ToRegionString()}:Posts").Get<string[]>())
			{
				Client.AddWebhookAsync(new(webhookLink)).GetAwaiter().GetResult();
			}
		}


		public async Task SendNewPostWebhookAsync(Post post, Player author, Player player)
		{
			DiscordEmbedBuilder embed = new()
			{
				Author = new() { Name = author.Username, Url = author.GetPlayerProfileLink() },
				Title = $"**New Post on {player.Username} :** \"{post.Title}\".",
				Url = new(post.GetPostProfileLink()),
				Footer = GetDefaultFooter(),
				Color = DiscordColor.Green
			};

			AddPostContent(embed, post);

			await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder().AddEmbed(embed));
		}

		public async Task SendEditedPostWebhookAsync(Post post, Player author, Player player)
		{
			DiscordEmbedBuilder embed = new()
			{
				Author = new() { Name = author.Username, Url = author.GetPlayerProfileLink() },
				Title = $"**Edited Post on {player.Username} :** \"{post.Title}\".",
				Url = new(post.GetPostProfileLink()),
				Footer = GetDefaultFooter(),
				Color = new(0xffc400) // Dark Yellow
			};

			AddPostContent(embed, post);

			await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder().AddEmbed(embed));
		}

		public async Task SendDeletedPostWebhookAsync(Post post, Player author, Player player)
		{
			DiscordEmbedBuilder embed = new()
			{
				Author = new() { Name = author.Username, Url = author.GetPlayerProfileLink() },
				Title = $"**Deleted Post on {player.Username} :** \"{post.Title}\".",
				Url = new(post.GetPostProfileLink()),
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

		private static DiscordEmbedBuilder AddPostContent(DiscordEmbedBuilder embed, Post post)
		{
			embed.Description = post.Content;
			embed.AddField("Performance", GetFlairValueString(post.ParsedFlairs?.Performance), true);
			embed.AddField("Teamplay", GetFlairValueString(post.ParsedFlairs?.Teamplay), true);
			embed.AddField("Courtesy", GetFlairValueString(post.ParsedFlairs?.Courtesy), true);

			return embed;
		}
	}
}
