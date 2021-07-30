using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Discord
{
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
				Title = $"**Post Locked by Mod-Action**",
				Url = new(modAction.Post.GetPostLink()),
				Footer = GetDefaultFooter(),
				Color = DiscordColor.Red
			};

			AddModActionContent(embed, modAction);

			await Client.BroadcastMessageAsync(GetCurrentRegionWebhookBuilder()
				.WithContent("**Single Post Deletion**")
				.AddEmbed(embed));
		}


		private static DiscordEmbedBuilder AddModActionContent(DiscordEmbedBuilder embed, PostModAction modAction)
		{
			embed.AddField("Moderated by", $"[{modAction.Mod?.Username ?? "Unknown"}]({modAction.Mod.GetPlayerProfileLink()})", true);
			embed.AddField("Post Author", $"[{modAction.Post.Author?.Username ?? "Unknown"}]({modAction.Post.Author?.GetPlayerProfileLink()})", true);
			embed.AddField("Reason", modAction.Reason, false);

			return embed;
		}
	}
}
