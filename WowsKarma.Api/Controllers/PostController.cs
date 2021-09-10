using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class PostController : ControllerBase
	{
		private readonly PlayerService playerService;
		private readonly PostService postService;

		public PostController(PlayerService playerService, PostService postService)
		{
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
			this.postService = postService ?? throw new ArgumentNullException(nameof(postService));
		}


		[HttpGet("{postId}")]
		public IActionResult GetPost(Guid postId)
			=> postService.GetPost(postId).Adapt<PlayerPostDTO>() is PlayerPostDTO post
				? !post.ModLocked || post.AuthorId == User.ToAccountListing()?.Id || User.IsInRole(ApiRoles.CM)
					? StatusCode(200, post)
					: StatusCode(410)
				: StatusCode(404);

		[HttpGet("{userId}/received")]
		public async Task<IActionResult> GetReceivedPosts(uint userId, [FromQuery] int? lastResults)
		{
			if (await playerService.GetPlayerAsync(userId) is null)
			{
				return StatusCode(404, $"Account {userId} not found");
			}

			IEnumerable<Post> posts = postService.GetReceivedPosts(userId);

			if (posts?.Count() is null or 0)
			{
				return StatusCode(204);
			}

			if (!User.IsInRole(ApiRoles.CM))
			{
				posts = posts.Where(p => !p.ModLocked || p.AuthorId == User.ToAccountListing().Id);
			}

			return base.StatusCode(200, posts.Adapt<List<PlayerPostDTO>>());
		}

		[HttpGet("{userId}/sent")]
		public async Task<IActionResult> GetSentPosts(uint userId, [FromQuery] int? lastResults)
		{
			if (await playerService.GetPlayerAsync(userId) is null)
			{
				return StatusCode(404, $"Account {userId} not found");
			}

			IEnumerable<Post> posts = postService.GetSentPosts(userId);

			if (posts?.Count() is null or 0)
			{
				return StatusCode(204);
			}

			if (User.ToAccountListing()?.Id != userId || !User.IsInRole(ApiRoles.CM))
			{
				posts = posts.Where(p => !p.ModLocked);
			}

			return StatusCode(200, posts.Adapt<List<PlayerPostDTO>>());
		}

		[HttpGet("latest")]
		public IActionResult GetLatestPosts([FromQuery] int count = 10)
		{
			AccountListingDTO currentUser = User.ToAccountListing();

			IQueryable<Post> posts = postService.GetLatestPosts();

			if (!User.IsInRole(ApiRoles.CM))
			{
				posts = posts.Where(p => !p.ModLocked || p.AuthorId == currentUser.Id);
			}

			return base.StatusCode(200,	posts.Take(count).Adapt<List<PlayerPostDTO>>());
		}

		[HttpPost, Authorize]
		public async Task<IActionResult> CreatePost([FromBody] PlayerPostDTO post, [FromQuery] bool ignoreChecks = false)
		{
			if (await playerService.GetPlayerAsync(post.AuthorId) is not Player author)
			{
				return StatusCode(404, $"Account {post.AuthorId} not found.");
			}

			if (await playerService.GetPlayerAsync(post.PlayerId) is not Player player)
			{
				return StatusCode(404, $"Account {post.PlayerId} not found.");
			}

			if (!ignoreChecks)
			{
				if (post.AuthorId != uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
				{
					return StatusCode(403, "Author is not authorized to post on behalf of other users.");
				}

				if (author.PostsBanned)
				{
					return StatusCode(403, "Post Author was banned from posting on this platform.");
				}
				if (author.OptedOut)
				{
					return StatusCode(403, "Post Author has opted-out from using this platform.");
				}

				if (player.OptedOut)
				{
					return StatusCode(403, "Targeted player has opted-out from using this platform.");
				}
			}
			else if (User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator))
			{
				return StatusCode(403, "Post Author is not authorized to bypass Post checks.");
			}

			try
			{
				await postService.CreatePostAsync(post, ignoreChecks);
				return StatusCode(201);
			}
			catch (ArgumentException e)
			{
				return StatusCode(400, e.ToString());
			}
		}

		[HttpPut, Authorize]
		public async Task<IActionResult> EditPost([FromBody] PlayerPostDTO post, [FromQuery] bool ignoreChecks = false)
		{
			if (postService.GetPost(post.Id ?? default) is not Post current)
			{
				return StatusCode(404, $"No post with ID {post.Id} found.");
			}

			if (!ignoreChecks)
			{
				if (current.AuthorId != uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
				{
					return StatusCode(403, "Author is not authorized to edit posts on behalf of other users.");
				}
				if (current.ModLocked)
				{
					return StatusCode(403, "Post has been locked by Community Managers. No modification is possible.");
				}
			}
			else if (User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator))
			{
				return StatusCode(403, "Post Author is not authorized to bypass Post checks.");
			}

			try
			{
				await postService.EditPostAsync(post.Id ?? default, post);
				return StatusCode(200);
			}
			catch (ArgumentException e)
			{
				return StatusCode(400, e.ToString());
			}
		}

		[HttpDelete("{postId}"), Authorize]
		public async Task<IActionResult> DeletePost(Guid postId, [FromQuery] bool ignoreChecks = false)
		{
			if (postService.GetPost(postId) is not Post post)
			{
				return StatusCode(404, $"No post with ID {postId} found.");
			}

			if (!ignoreChecks)
			{
				if (post.AuthorId != uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
				{
					return StatusCode(403, "Author is not authorized to delete posts on behalf of other users.");
				}
				if (post.ModLocked)
				{
					return StatusCode(403, "Post has been locked by Community Managers. No deletion is possible.");
				}
			}
			else if (User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator))
			{
				return StatusCode(403, "Post Author is not authorized to bypass Post checks.");
			}

			try
			{
				await postService.DeletePostAsync(postId);
				return StatusCode(205);
			}
			catch (ArgumentException e)
			{
				return StatusCode(400, e.ToString());
			}
		}

		[HttpGet("model")]
		public IActionResult GetPostDTOModel() => StatusCode(200, new PlayerPostDTO());
	}
}
