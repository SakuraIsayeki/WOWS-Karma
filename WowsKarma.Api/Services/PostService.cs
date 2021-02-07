using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class PostService
	{
		private const ushort PostTitleMaxSize = 60;
		private const ushort PostContentMaxSize = 1000;
		private static readonly TimeSpan CooldownPeriod = TimeSpan.FromDays(1);

		private readonly ApiDbContext context;
		private readonly PlayerService playerService;
		private readonly KarmaService karmaService;


		public PostService(IDbContextFactory<ApiDbContext> contextFactory, PlayerService playerService, KarmaService karmaService)
		{
			context = contextFactory.CreateDbContext() ?? throw new ArgumentNullException(nameof(contextFactory));
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
			this.karmaService = karmaService ?? throw new ArgumentNullException(nameof(karmaService));
		}


		public IEnumerable<Post> GetReceivedPosts(uint playerId, int lastResults)
		{
			Player player = context.Players
				.Include(p => p.PostsReceived).ThenInclude(p => p.Player)
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
				.Include(p => p.PostsSent).ThenInclude(p => p.Author)
				.FirstOrDefault(p => p.Id == authorId);

			return author?.PostsSent is not null
				? lastResults > 0 && lastResults < author.PostsSent.Count
					? author.PostsSent.OrderBy(p => p.CreatedAt).TakeLast(lastResults)
					: author.PostsSent.OrderBy(p => p.CreatedAt)
				: null;
		}

		public async Task CreatePostAsync(PlayerPostDTO postDTO, bool bypassCooldown)
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
			_ = await playerService.GetPlayerAsync(postDTO.PlayerId) ?? throw new ArgumentException($"Player Account {postDTO.PlayerId} not found", nameof(postDTO));

			if (!bypassCooldown && CheckCooldown(postDTO))
			{
				throw new ArgumentException("Author is on cooldown for this player.");
			}


			Post post = new()
			{
				AuthorId = postDTO.AuthorId,
				PlayerId = postDTO.PlayerId,
				Title = postDTO.Title,
				Content = postDTO.Content,
				Flairs = postDTO.Flairs,
				NegativeKarmaAble = author.NegativeKarmaAble,
			};

			context.Posts.Add(post);
			await context.SaveChangesAsync();

			await karmaService.UpdatePlayerKarmaAsync(post.PlayerId, post.ParsedFlairs, null, post.NegativeKarmaAble);
			await karmaService.UpdatePlayerRatingsAsync(post.PlayerId, post.ParsedFlairs, null);
		}

		public async Task EditPostAsync(Guid id, PlayerPostDTO editedPostDTO)
		{
			ValidatePostContents(editedPostDTO);

			Post post = await context.Posts.FindAsync(id) ?? throw new ArgumentException($"No post with ID {id} found.", nameof(id));
			PostFlairsParsed previousFlairs = post.ParsedFlairs;

			post.Title = editedPostDTO.Title;
			post.Content = editedPostDTO.Content;
			post.Flairs = editedPostDTO.Flairs;
			post.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh
			await context.SaveChangesAsync();

			await karmaService.UpdatePlayerKarmaAsync(post.PlayerId, post.ParsedFlairs, previousFlairs, post.NegativeKarmaAble);
			await karmaService.UpdatePlayerRatingsAsync(post.PlayerId, post.ParsedFlairs, previousFlairs);
		}

		public async Task DeletePostAsync(Guid id)
		{
			Post post = await context.Posts.FindAsync(id) ?? throw new ArgumentException($"No post with ID {id} found.", nameof(id));
			context.Posts.Remove(post);

			await karmaService.UpdatePlayerKarmaAsync(post.PlayerId, null, post.ParsedFlairs, post.NegativeKarmaAble);
			await karmaService.UpdatePlayerRatingsAsync(post.PlayerId, null, post.ParsedFlairs);
			await context.SaveChangesAsync();
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
				PlayerPostDTO lastAuthoredPost = filteredPosts.OrderBy(p => p.CreatedAt).LastOrDefault();

				if (lastAuthoredPost is not null)
				{
					DateTime endsAt = lastAuthoredPost.PostedAt.Value.Add(CooldownPeriod);
					return endsAt > DateTime.UtcNow;

				}
			}
			return false;
		}
	}
}
