using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using WowsKarma.Common.Models;

namespace WowsKarma.Api.Services.Discord
{
	public class PostWebhookService
	{
		private readonly DiscordWebhookClient client;

		public PostWebhookService(IConfiguration configuration)
		{
			client = new();

			foreach (string webhookLink in configuration.GetSection($"Webhooks:Discord:{Startup.ApiRegion.ToRegionString()}:Posts").Get<string[]>())
			{
				client.AddWebhookAsync(new(webhookLink)).GetAwaiter().GetResult();
			}
		}

		public async Task SendNewPostWebhook(Post post, Player author, Player player)
		{
			DiscordEmbedBuilder embed = new()
			{
				Author = new() { Name = author.Username, Url = author.GetPlayerProfileLink() },
				Title = $"**New Post on {player.Username} :** \"{post.Title}\".",
				Url = new(player.GetPlayerProfileLink()),
				Description = post.Content,
				Footer = new() { Text = $"WOWS Karma ({Startup.ApiRegion.ToRegionString()}) - Powered by Nodsoft Systems" },

				Color = PostFlairsUtils.CountBalance(post.Flairs.ParseFlairsEnum()) switch
				{
					> 0 => DiscordColor.Green,
					< 0 => DiscordColor.Red,
					_ => DiscordColor.LightGray
				}
			};

			embed.AddField("Performance", GetFlairValueString(post.ParsedFlairs.Performance), true);
			embed.AddField("Teamplay", GetFlairValueString(post.ParsedFlairs.Teamplay), true);
			embed.AddField("Courtesy", GetFlairValueString(post.ParsedFlairs.Courtesy), true);

			await client.BroadcastMessageAsync(new DiscordWebhookBuilder().AddEmbed(embed));
		}

		private static string GetFlairValueString(bool? value) => value switch
		{
			true => "Positive",
			false => "Negative",
			null or _ => "Neutral"
		};
	}
}
