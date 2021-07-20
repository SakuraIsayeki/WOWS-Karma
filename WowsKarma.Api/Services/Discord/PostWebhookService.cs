using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using WowsKarma.Common.Models;

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

		public async Task SendNewPostWebhook(Post post, Player author, Player player)
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
