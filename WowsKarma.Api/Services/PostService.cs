using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Services
{
	public class PostService
	{
		private const ushort PostTitleMaxSize = 60;
		private const ushort PostContentMaxSize = 1000;

		private readonly ApiDbContext context;
		private readonly PlayerService playerService;

		public PostService(IDbContextFactory<ApiDbContext> contextFactory, PlayerService playerService)
		{
			context = contextFactory.CreateDbContext() ?? throw new ArgumentNullException(nameof(contextFactory));
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
		}


		public IEnumerable<Post> GetReceivedPosts(uint playerId, int lastResults)
		{
			Player player = context.Players.Include(p => p.PostsReceived).FirstOrDefault(p => p.Id == playerId);

			return player.PostsReceived is not null
				? lastResults > 0 && lastResults < player.PostsReceived.Count()
					? player.PostsReceived.OrderBy(p => p.CreatedAt).TakeLast(lastResults)
					: player.PostsReceived.OrderBy(p => p.CreatedAt)
				: null;
		}

		public IEnumerable<Post> GetSentPosts(uint authorId, int lastResults)
		{
			Player author = context.Players.Include(p => p.PostsSent).FirstOrDefault(p => p.Id == authorId);

			return author?.PostsSent is not null
				? lastResults > 0 && lastResults < author.PostsSent.Count()
					? author.PostsSent.OrderBy(p => p.CreatedAt).TakeLast(lastResults)
					: author.PostsSent.OrderBy(p => p.CreatedAt)
				: null;
		}

		public async Task CreatePostAsync(PlayerPostDTO postDTO)
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

			Post post = new()
			{
				AuthorId = postDTO.AuthorId,
				PlayerId = postDTO.PlayerId,
				Title = postDTO.Title,
				Content = postDTO.Content,
				PostFlairs = postDTO.PostFlairs,
				NegativeKarmaAble = author.NegativeKarmaAble,
			};

			await AddPostAsync(post);
		}

		public async Task EditPostAsync(Guid id, PlayerPostDTO editedPostDTO)
		{
			ValidatePostContents(editedPostDTO);

			Post post = await context.Posts.FindAsync(id) ?? throw new ArgumentException($"No post with ID {id} found.", nameof(id));

			post.Title = editedPostDTO.Title;
			post.Content = editedPostDTO.Content;
			post.PostFlairs = editedPostDTO.PostFlairs;
			post.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh

			await context.SaveChangesAsync();
			
		}

		public async Task DeletePostAsync(Guid id)
		{
			Post post = await context.Posts.FindAsync(id) ?? throw new ArgumentException($"No post with ID {id} found.", nameof(id));
			context.Posts.Remove(post);

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

		protected async Task AddPostAsync(Post post)
		{
			context.Posts.Add(post);
			await context.SaveChangesAsync();
		}
	}
}
