using Discord;
using Discord.Webhook;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Utilities;
using WowsKarma.Common;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class PostService
	{
		private const ushort PostTitleMaxSize = 60;
		private const ushort PostContentMaxSize = 2000;
		private static readonly TimeSpan CooldownPeriod = TimeSpan.FromDays(1);

		private readonly ApiDbContext context;
		private readonly PlayerService playerService;
		private readonly KarmaService karmaService;
		private readonly DiscordWebhookClient webhookClient;
		private readonly IHubContext<PostHub> hubContext;

		public PostService(IDbContextFactory<ApiDbContext> contextFactory, PlayerService playerService, KarmaService karmaService, DiscordWebhookClient webhookClient, IHubContext<PostHub> hubContext)
		{
			context = contextFactory.CreateDbContext() ?? throw new ArgumentNullException(nameof(contextFactory));
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
			this.karmaService = karmaService ?? throw new ArgumentNullException(nameof(karmaService));
			this.webhookClient = webhookClient;
			this.hubContext = hubContext;
		}

		public Post GetPost(Guid id) => context.Posts
			.Include(p => p.Author)
			.Include(p => p.Player)
			.FirstOrDefault(p => p.Id == id);

		public IEnumerable<Post> GetReceivedPosts(uint playerId, int lastResults)
		{
			Player player = context.Players
				.Include(p => p.PostsReceived).ThenInclude(p => p.Author)
				.FirstOrDefault(p => p.Id == playerId);

			return player.PostsReceived is not null
				? lastResults > 0 && lastResults < player.PostsReceived.Count
					? player.PostsReceived.OrderBy(p => p.CreatedAt).TakeLast(lastResults)
					: player.PostsReceived.OrderBy(p => p.CreatedAt)
				: null;
		}

		public IEnumerable<Post> GetSentPosts(uint authorId, int lastResults)
		{
			Player author = context.Players
				.Include(p => p.PostsSent).ThenInclude(p => p.Player)
				.FirstOrDefault(p => p.Id == authorId);

			return author?.PostsSent is not null
				? lastResults > 0 && lastResults < author.PostsSent.Count
					? author.PostsSent.OrderBy(p => p.CreatedAt).TakeLast(lastResults)
					: author.PostsSent.OrderBy(p => p.CreatedAt)
				: null;
		}

		public IEnumerable<Post> GetLatestPosts(int count) => context.Posts
			.Include(p => p.Author)
			.Include(p => p.Player)
			.OrderByDescending(p => p.CreatedAt)
			.Take(count);

		public async Task CreatePostAsync(PlayerPostDTO postDTO, bool bypassChecks)
		{
			try
			{
				ValidatePostContents(postDTO);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Validation failed.", nameof(postDTO), e);
			}

			Player author = await playerService.GetPlayerAsync(postDTO.AuthorId) ?? throw new ArgumentException($"Author Account {postDTO.AuthorId} not found", nameof(postDTO));
			Player player = await playerService.GetPlayerAsync(postDTO.PlayerId) ?? throw new ArgumentException($"Player Account {postDTO.PlayerId} not found", nameof(postDTO));

			if (!bypassChecks)
			{
				if (player.OptedOut)
				{
					throw new ArgumentException("Player has opted-out of this platform.");
				}

				if (CheckCooldown(postDTO))
				{
					throw new ArgumentException("Author is on cooldown for this player.");
				}
			}


			Post post = postDTO.Adapt<Post>();

			context.Posts.Add(post);
			await context.SaveChangesAsync();

			await karmaService.UpdatePlayerKarmaAsync(post.PlayerId, post.ParsedFlairs, null, post.NegativeKarmaAble);
			await karmaService.UpdatePlayerRatingsAsync(post.PlayerId, post.ParsedFlairs, null);

			await SendDiscordWebhookMessage(post, author, player);
			await hubContext.Clients.All.SendAsync("NewPost", post.Adapt<PlayerPostDTO>() with 
			{ 
				AuthorUsername = author.Username,
				PlayerUsername = player.Username
			});
		}

		public async Task EditPostAsync(Guid id, PlayerPostDTO editedPostDTO)
		{
			ValidatePostContents(editedPostDTO);

			Post post = await context.Posts.FindAsync(id);
			PostFlairsParsed previousFlairs = post.ParsedFlairs;

			post.Title = editedPostDTO.Title;
			post.Content = editedPostDTO.Content;
			post.Flairs = editedPostDTO.Flairs;
			post.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh
			await context.SaveChangesAsync();

			await karmaService.UpdatePlayerKarmaAsync(post.PlayerId, post.ParsedFlairs, previousFlairs, post.NegativeKarmaAble);
			await karmaService.UpdatePlayerRatingsAsync(post.PlayerId, post.ParsedFlairs, previousFlairs);

			await hubContext.Clients.All.SendAsync("EditedPost", post.Adapt<PlayerPostDTO>() with
			{
				AuthorUsername = post.Author?.Username,
				PlayerUsername = post.Player?.Username
			});
		}

		public async Task DeletePostAsync(Guid id)
		{
			Post post = await context.Posts.FindAsync(id);
			context.Posts.Remove(post);

			await karmaService.UpdatePlayerKarmaAsync(post.PlayerId, null, post.ParsedFlairs, post.NegativeKarmaAble);
			await karmaService.UpdatePlayerRatingsAsync(post.PlayerId, null, post.ParsedFlairs);
			await context.SaveChangesAsync();

			await hubContext.Clients.All.SendAsync("DeletedPost", id);
		}

		internal static void ValidatePostContents(PlayerPostDTO post)
		{
			_ = post ?? throw new ArgumentNullException(nameof(post));

			_ = string.IsNullOrWhiteSpace(post.Title) ? throw new ArgumentException("Post Title is Empty", nameof(post)) : false;
			_ = string.IsNullOrWhiteSpace(post.Content) ? throw new ArgumentException("Post Content is Empty", nameof(post)) : false;

			_ = post.Title.Length > PostTitleMaxSize ? throw new ArgumentException($"Post Title Length exceeds {PostTitleMaxSize} characters", nameof(post)) : false;
			_ = post.Content.Length > PostContentMaxSize ? throw new ArgumentException($"Post Content Length exceeds {PostContentMaxSize} characters", nameof(post)) : false;
		}


		private bool CheckCooldown(PlayerPostDTO post)
		{
			IQueryable<Post> filteredPosts =
				 from p in context.Posts
				 where p.AuthorId == post.AuthorId
				 where p.PlayerId == post.PlayerId
				 select p;

			if (filteredPosts.Any())
			{
				PlayerPostDTO lastAuthoredPost = filteredPosts.OrderBy(p => p.CreatedAt).LastOrDefault().Adapt<PlayerPostDTO>();

				if (lastAuthoredPost is not null)
				{
					DateTime endsAt = lastAuthoredPost.CreatedAt.Value.Add(CooldownPeriod);
					return endsAt > DateTime.UtcNow;

				}
			}
			return false;
		}

		private async Task SendDiscordWebhookMessage(Post post, Player author, Player player)
		{
			EmbedBuilder embed = new()
			{
				Author = new() { Name = author.Username, Url = author.GetPlayerProfileLink() },
				Title = $"**New Post on {player.Username} :** \"{post.Title}\".",
				Url = new(player.GetPlayerProfileLink()),
				Description = post.Content,
				Footer = new() { Text = $"WOWS Karma ({Startup.ApiRegion.ToRegionString()}) - Powered by Nodsoft Systems" },

				Color = PostFlairsUtils.CountBalance(post.Flairs.ParseFlairsEnum()) switch
				{
					> 0 => Color.Green,
					< 0 => Color.Red,
					_ => Color.LighterGrey
				},

				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder { Name = "Performance", Value = GetFlairValueString(post.ParsedFlairs.Performance), IsInline = true },
					new EmbedFieldBuilder { Name = "Teamplay", Value = GetFlairValueString(post.ParsedFlairs.Teamplay), IsInline = true },
					new EmbedFieldBuilder { Name = "Courtesy", Value = GetFlairValueString(post.ParsedFlairs.Courtesy), IsInline = true }
				}
			};

			await webhookClient.SendMessageAsync(embeds: new Embed[] { embed.Build() });
		}

		private static string GetFlairValueString(bool? value) => value switch
		{
			true => "Positive",
			false => "Negative",
			null or _ => "Neutral"
		};
	}
}
