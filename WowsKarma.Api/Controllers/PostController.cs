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

		/// <summary>
		/// Fetches player post with given ID
		/// </summary>
		/// <param name="postId">Post's GUID</param>
		/// <response code="200">Returns <see cref="PlayerPostDTO"/> object of Post with specified ID</response>
		/// <response code="404">No post was found with given ID.</response>
		/// <response code="410">Post is locked by Community Managers.</response>
		[HttpGet("{postId}"), ProducesResponseType(typeof(PlayerPostDTO), 200), ProducesResponseType(404), ProducesResponseType(410)]
		public IActionResult GetPost(Guid postId)
			=> postService.GetPost(postId).Adapt<PlayerPostDTO>() is PlayerPostDTO post
				? !post.ModLocked || post.AuthorId == User.ToAccountListing()?.Id || User.IsInRole(ApiRoles.CM)
					? StatusCode(200, post)
					: StatusCode(410)
				: StatusCode(404);

		/// <summary>
		/// Fetches all posts that a given player has received.
		/// </summary>
		/// <param name="userId">Account ID of player to query</param>
		/// <param name="lastResults">Return maximum of results (where available)</param>
		/// <response code="200">List of posts received by player</response>
		/// <response code="204">No posts received for given player.</response>
		/// <response code="404">No player found for given Account ID.</response>
		[HttpGet("{userId}/received"), ProducesResponseType(typeof(IEnumerable<PlayerPostDTO>), 200), ProducesResponseType(204), ProducesResponseType(404)]
		public async Task<IActionResult> GetReceivedPosts(uint userId, [FromQuery] int? lastResults)
		{
			if (await playerService.GetPlayerAsync(userId) is null)
			{
				return StatusCode(404, $"Account {userId} not found");
			}

			IEnumerable<Post> posts = postService.GetReceivedPosts(userId);

			if (!User.IsInRole(ApiRoles.CM))
			{
				posts = posts?.Where(p => !p.ModLocked || p.AuthorId == User.ToAccountListing().Id);
			}

			if (posts?.Count() is null or 0)
			{
				return StatusCode(204);
			}

			return base.StatusCode(200, posts.Adapt<List<PlayerPostDTO>>());
		}

		/// <summary>
		/// Fetches all posts that a given player has sent.
		/// </summary>
		/// <param name="userId">Account ID of player to query</param>
		/// <param name="lastResults">Return maximum of results (where available)</param>
		/// <response code="200">List of posts sent by player</response>
		/// <response code="204">No posts sent by given player.</response>
		/// <response code="404">No player found for given Account ID.</response>
		[HttpGet("{userId}/sent"), ProducesResponseType(typeof(IEnumerable<PlayerPostDTO>), 200), ProducesResponseType(204), ProducesResponseType(404)]
		public async Task<IActionResult> GetSentPosts(uint userId, [FromQuery] int? lastResults)
		{
			if (await playerService.GetPlayerAsync(userId) is null)
			{
				return StatusCode(404, $"Account {userId} not found");
			}

			IEnumerable<Post> posts = postService.GetSentPosts(userId);

			if (User.ToAccountListing()?.Id != userId || !User.IsInRole(ApiRoles.CM))
			{
				posts = posts?.Where(p => !p.ModLocked);
			}

			if (posts?.Count() is null or 0)
			{
				return StatusCode(204);
			}

			return StatusCode(200, posts.Adapt<List<PlayerPostDTO>>());
		}

		/// <summary>
		/// Fetches latest posts.
		/// </summary>
		/// <param name="count">Return maximum of results.</param>
		/// <response code="200">List of latest posts, sorted by Submission time.</response>
		[HttpGet("latest"), ProducesResponseType(typeof(IEnumerable<PlayerPostDTO>), 200)]
		public IActionResult GetLatestPosts([FromQuery] int count = 10)
		{
			AccountListingDTO currentUser = User.ToAccountListing();

			IQueryable<Post> posts = postService.GetLatestPosts();

			if (!User.IsInRole(ApiRoles.CM))
			{
				posts = posts.Where(p => !p.ModLocked || p.AuthorId == currentUser.Id);
			}

			return base.StatusCode(200, posts.Take(count).Adapt<List<PlayerPostDTO>>());
		}

		/// <summary>
		/// Submits a new post for creation.
		/// </summary>
		/// <param name="post">Post object to submit</param>
		/// <param name="ignoreChecks">Bypass API Validation for post creation (Admin only).</param>
		/// <response code="201">Post was successfuly created.</response>
		/// <response code="400">Post contents validation has failed.</response>
		/// <response code="403">Restrictions are in effect for one of the targeted accounts.</response>
		/// <response code="404">One of the targeted accounts was not found.</response>
		[HttpPost, Authorize, ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
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

			if (ignoreChecks)
			{
				if (!(User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator)))
				{
					return StatusCode(403, "Post Author is not authorized to bypass Post checks.");
				}
			}
			else
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

			try
			{
				await postService.CreatePostAsync(post, ignoreChecks);
				return StatusCode(201);
			}
			catch (ArgumentException e)
			{
				return BadRequest(e);
			}
		}

		/// <summary>
		/// Submits an updated post for editing.
		/// </summary>
		/// <param name="post">Post object to submit</param>
		/// <param name="ignoreChecks">Bypass API Validation for post editing (Admin only).</param>
		/// <response code="201">Post was successfuly edited.</response>
		/// <response code="400">Post contents validation has failed.</response>
		/// <response code="403">Restrictions are in effect for the existing post.</response>
		/// <response code="404">Targeted post was not found.</response>
		[HttpPut, Authorize, ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(403), ProducesResponseType(404)]
		public async Task<IActionResult> EditPost([FromBody] PlayerPostDTO post, [FromQuery] bool ignoreChecks = false)
		{
			if (postService.GetPost(post.Id ?? default) is not Post current)
			{
				return StatusCode(404, $"No post with ID {post.Id} found.");
			}

			if (ignoreChecks)
			{
				if (!(User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator)))
				{
					return StatusCode(403, "Post Author is not authorized to bypass Post checks.");
				}
			}
			else
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

		/// <summary>
		/// Requests a post deletion.
		/// </summary>
		/// <param name="postId">ID of Post to delete</param>
		/// <param name="ignoreChecks">Bypass API Validation for post deletion (Admin only).</param>
		/// <response code="205">Post was successfuly deleted.</response>
		/// <response code="403">Restrictions are in effect for the existing post.</response>
		/// <response code="404">Targeted post was not found.</response>
		[HttpDelete("{postId}"), Authorize, ProducesResponseType(205), ProducesResponseType(403), ProducesResponseType(404)]
		public async Task<IActionResult> DeletePost(Guid postId, [FromQuery] bool ignoreChecks = false)
		{
			if (postService.GetPost(postId) is not Post post)
			{
				return StatusCode(404, $"No post with ID {postId} found.");
			}

			if (ignoreChecks)
			{
				if (!(User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator)))
				{
					return StatusCode(403, "Post Author is not authorized to bypass Post checks.");
				}
			}
			else
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

			await postService.DeletePostAsync(postId);
			return StatusCode(205);
		}


		/// <summary>
		/// Responds with a blank <see cref="PlayerPostDTO"/> object.
		/// </summary>
		/// <response code="200">Returns a new object.</response>
		[HttpGet("model"), ProducesResponseType(typeof(PlayerPostDTO), 200)]
		public IActionResult GetPostDTOModel() => StatusCode(200, new PlayerPostDTO());
	}
}
