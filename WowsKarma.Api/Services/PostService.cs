using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly ApiDbContext context;
		private readonly PlayerService playerService;

		public delegate void PostAddedEventHandler(object sender, PostEventArgs e);
		public delegate void PostAddedOrUpdatedEventHandler(object sender, PostUpdatedEventArgs e);
		public delegate void PostDeletedHandler(object sender, PostEventArgs e);

		public static event PostAddedEventHandler PostAdded;
		public static event PostAddedOrUpdatedEventHandler PostUpdated;
		public static event PostDeletedHandler PostDeleted;



		public PostService(IDbContextFactory<ApiDbContext> contextFactory, PlayerService playerService)
		{
			context = contextFactory.CreateDbContext() ?? throw new ArgumentNullException(nameof(contextFactory));
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
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
				Flairs = postDTO.Flairs,
				NegativeKarmaAble = author.NegativeKarmaAble,
			};

			context.Posts.Add(post);
			OnPostAdded(new() { Post = post });

			await context.SaveChangesAsync();
		}

		public async Task EditPostAsync(Guid id, PlayerPostDTO editedPostDTO)
		{
			ValidatePostContents(editedPostDTO);

			Post post = await context.Posts.FindAsync(id) ?? throw new ArgumentException($"No post with ID {id} found.", nameof(id));
			PostUpdatedEventArgs eventArgs = new() { PreviousFlairs = post.ParsedFlairs };

			post.Title = editedPostDTO.Title;
			post.Content = editedPostDTO.Content;
			post.Flairs = editedPostDTO.Flairs;
			post.UpdatedAt = DateTime.UtcNow; // Forcing UpdatedAt refresh

			eventArgs.Post = post;
			OnPostUpdated(eventArgs);
			await context.SaveChangesAsync();
		}

		public async Task DeletePostAsync(Guid id)
		{
			Post post = await context.Posts.FindAsync(id) ?? throw new ArgumentException($"No post with ID {id} found.", nameof(id));
			context.Posts.Remove(post);
			OnPostDeleted(new() { Post = post });
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

		protected virtual void OnPostAdded(PostEventArgs e) => PostAdded?.Invoke(this, e);
		protected virtual void OnPostUpdated(PostUpdatedEventArgs e) => PostUpdated?.Invoke(this, e);
		protected virtual void OnPostDeleted(PostEventArgs e) => PostDeleted?.Invoke(this, e);
	}

	public class PostEventArgs : EventArgs
	{
		public Post Post { get; set; }
	}
	public class PostUpdatedEventArgs : PostEventArgs
	{ 
		public PostFlairsParsed PreviousFlairs { get; set; }
	}
}
