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
using WowsKarma.Api.Services.Discord;
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
		private readonly PostWebhookService webhookService;
		private readonly IHubContext<PostHub> hubContext;

		public PostService(IDbContextFactory<ApiDbContext> contextFactory, PlayerService playerService, PostWebhookService webhookService, IHubContext<PostHub> hubContext)
		{
			context = contextFactory.CreateDbContext() ?? throw new ArgumentNullException(nameof(contextFactory));
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
			this.webhookService = webhookService;
			this.hubContext = hubContext;
		}

		public Post GetPost(Guid id) => context.Posts
			.Include(p => p.Author)
			.Include(p => p.Player)
			.FirstOrDefault(p => p.Id == id);

		public IEnumerable<Post> GetReceivedPosts(uint playerId)
		{
			Player player = context.Players
				.Include(p => p.PostsReceived).ThenInclude(p => p.Author)
				.FirstOrDefault(p => p.Id == playerId);

			return player.PostsReceived?.OrderBy(p => p.CreatedAt);
		}

		public IEnumerable<Post> GetSentPosts(uint authorId)
		{
			Player author = context.Players
				.Include(p => p.PostsSent).ThenInclude(p => p.Player)
				.FirstOrDefault(p => p.Id == authorId);

			return author?.PostsSent?.OrderBy(p => p.CreatedAt);
		}

		public IQueryable<Post> GetLatestPosts(int? count) => context.Posts
			.Include(p => p.Author)
			.Include(p => p.Player)
			.OrderByDescending(p => p.CreatedAt);

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
			Player player = await context.Players.FindAsync(postDTO.PlayerId) ?? throw new ArgumentException($"Player Account {postDTO.PlayerId} not found", nameof(postDTO));

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
			post.NegativeKarmaAble = author.NegativeKarmaAble;

			context.Posts.Add(post);
			KarmaService.UpdatePlayerKarma(ref player, post.ParsedFlairs, null, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(ref player, post.ParsedFlairs, null);
			
			await context.SaveChangesAsync();

			_ = webhookService.SendNewPostWebhookAsync(post, author, player);

			_ = hubContext.Clients.All.SendAsync("NewPost", post.Adapt<PlayerPostDTO>() with 
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
			Player player = await context.Players.FindAsync(post.PlayerId) ?? throw new ArgumentException($"Player Account {editedPostDTO.PlayerId} not found", nameof(editedPostDTO));

			post.Title = editedPostDTO.Title;
			post.Content = editedPostDTO.Content;
			post.Flairs = editedPostDTO.Flairs;
			post.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh

			KarmaService.UpdatePlayerKarma(ref player, post.ParsedFlairs, previousFlairs, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(ref player, post.ParsedFlairs, previousFlairs);
			
			await context.SaveChangesAsync();

			_ = webhookService.SendEditedPostWebhookAsync(post, await playerService.GetPlayerAsync(post.AuthorId), player);

			_ = hubContext.Clients.All.SendAsync("EditedPost", post.Adapt<PlayerPostDTO>() with
			{
				AuthorUsername = post.Author?.Username,
				PlayerUsername = post.Player?.Username
			});
		}

		public async Task DeletePostAsync(Guid id, bool modLock = false)
		{
			Post post = await context.Posts.FindAsync(id);
			Player player = await context.Players.FindAsync(post.PlayerId) ?? throw new ArgumentException($"Player Account {post.PlayerId} not found");

			if (modLock)
			{
				post.ModLocked = true;
			}
			else
			{ 
				context.Posts.Remove(post);
			}

			KarmaService.UpdatePlayerKarma(ref player, null, post.ParsedFlairs, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(ref player, null, post.ParsedFlairs);
			
			await context.SaveChangesAsync();

			if (!modLock)
			{
				_ = webhookService.SendDeletedPostWebhookAsync(post, await playerService.GetPlayerAsync(post.AuthorId), player);
			}

			_ = hubContext.Clients.All.SendAsync("DeletedPost", id);
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
	}
}
