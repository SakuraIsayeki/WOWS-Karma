using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Hubs;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Services.Discord;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Common.Hubs;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Common.Models.DTOs.Notifications;

namespace WowsKarma.Api.Services
{
	public class PostService
	{
		public const ushort PostTitleMaxSize = 60;
		public const ushort PostContentMaxSize = 2000;
		public static TimeSpan CooldownPeriod { get; } = TimeSpan.FromDays(1);

		private readonly ApiDbContext context;
		private readonly PlayerService playerService;
		private readonly PostWebhookService webhookService;
		private readonly IHubContext<PostHub, IPostHubPush> _postsHub;
		private readonly NotificationService _notificationService;
		private readonly ReplaysIngestService _replayService;

		public PostService(ApiDbContext context, PlayerService playerService, PostWebhookService webhookService, IHubContext<PostHub, IPostHubPush> postsHub,
			NotificationService notificationService, ReplaysIngestService replayService)
		{
			this.context = context;
			this.playerService = playerService;
			this.webhookService = webhookService;
			_postsHub = postsHub;
			_notificationService = notificationService;
			_replayService = replayService;
		}

		public Post GetPost(Guid id) => context.Posts
			.Include(p => p.Author)
			.Include(p => p.Player)
			.Include(p => p.Replay)
			.FirstOrDefault(p => p.Id == id);

		public async Task<PlayerPostDTO> GetPostDTOAsync(Guid id)
		{
			Post post = GetPost(id);
			PlayerPostDTO postDTO = post?.Adapt<PlayerPostDTO>();

			return post?.ReplayId is null
				? postDTO
				: postDTO with
				{
					Replay = await _replayService.GetReplayDTOAsync(post.ReplayId.Value)
				};
		}

		public IEnumerable<Post> GetReceivedPosts(uint playerId) => context.Players.AsNoTracking()
			.Include(p => p.PostsReceived)
				.ThenInclude(p => p.Author)
			.FirstOrDefault(p => p.Id == playerId)?
			.PostsReceived?.OrderBy(p => p.CreatedAt);

		public IEnumerable<Post> GetSentPosts(uint authorId) => context.Players.AsNoTracking()
			.Include(p => p.PostsSent)
				.ThenInclude(p => p.Player)
			.FirstOrDefault(p => p.Id == authorId)?
			.PostsSent?.OrderBy(p => p.CreatedAt);

		public IQueryable<Post> GetLatestPosts() => context.Posts.AsNoTracking()
			.Include(p => p.Author)
			.Include(p => p.Player)
			.OrderByDescending(p => p.CreatedAt);

		public async Task<Post> CreatePostAsync(PlayerPostDTO postDTO, IFormFile replayFile, bool bypassChecks)
		{
			bool hasReplay = replayFile is not null;

			Task<Replay> replayIngestTask = hasReplay ? _replayService.IngestReplayAsync(replayFile, CancellationToken.None) : null;

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
					throw new CooldownException("Author is on cooldown for this player.");
				}
			}

			Post post = postDTO.Adapt<Post>();
			post.NegativeKarmaAble = author.NegativeKarmaAble;

			EntityEntry<Post> entry = context.Posts.Add(post);
			KarmaService.UpdatePlayerKarma(player, post.ParsedFlairs, null, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(player, post.ParsedFlairs, null);

			if (hasReplay)
			{
				Replay replay = await replayIngestTask;

				entry.Entity.ReplayId = replay.Id;
				entry.Entity.Replay = replay; 
			}

			await context.SaveChangesAsync();

			_ = webhookService.SendNewPostWebhookAsync(post, author, player);

			_ = _postsHub.Clients.All.NewPost(post.Adapt<PlayerPostDTO>() with
			{
				AuthorUsername = author.Username,
				PlayerUsername = player.Username
			});

			_ = _notificationService.SendNewNotification(new PostAddedNotification
			{
				Account = player,
				AccountId = player.Id,
				Post = post,
				PostId = post.Id,
			});

			return entry.Entity;
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

			KarmaService.UpdatePlayerKarma(player, post.ParsedFlairs, previousFlairs, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(player, post.ParsedFlairs, previousFlairs);

			await context.SaveChangesAsync();

			_ = webhookService.SendEditedPostWebhookAsync(post, await playerService.GetPlayerAsync(post.AuthorId), player);

			_ = _postsHub.Clients.All.EditedPost(post.Adapt<PlayerPostDTO>() with
			{
				AuthorUsername = post.Author?.Username,
				PlayerUsername = post.Player?.Username
			});

			_ = _notificationService.SendNewNotification(new PostEditedNotification
			{
				Account = player,
				AccountId = player.Id,
				Post = post,
				PostId = post.Id,
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

			KarmaService.UpdatePlayerKarma(player, null, post.ParsedFlairs, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(player, null, post.ParsedFlairs);

			await context.SaveChangesAsync();

			if (!modLock)
			{
				_ = webhookService.SendDeletedPostWebhookAsync(post, await playerService.GetPlayerAsync(post.AuthorId), player);
			}

			_ = _postsHub.Clients.All.DeletedPost(id);

			_ = _notificationService.SendNewNotification(new PostDeletedNotification
			{
				Account = player,
				AccountId = player.Id,
				PostId = post.Id,
			});
		}

		public async Task RevertPostModLockAsync(Guid id)
		{
			Post post = await context.Posts.FindAsync(id);
			post.ModLocked = false;

			Player player = await context.Players.FindAsync(post.PlayerId) ?? throw new ArgumentException($"Player Account {post.PlayerId} not found");
			KarmaService.UpdatePlayerKarma(player, post.ParsedFlairs, null, post.NegativeKarmaAble);
			KarmaService.UpdatePlayerRatings(player, post.ParsedFlairs, null);

			await context.SaveChangesAsync();

			_ = _postsHub.Clients.All.DeletedPost(id);

			_ = _notificationService.SendNewNotification(new PostEditedNotification
			{
				AccountId = post.PlayerId,
				PostId = post.Id,
			});

			_ = _notificationService.SendNewNotification(new PostEditedNotification
			{
				AccountId = post.AuthorId,
				PostId = post.Id,
			});
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
