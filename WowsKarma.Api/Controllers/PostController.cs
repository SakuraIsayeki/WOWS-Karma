using System.Security;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WowsKarma.Api.Infrastructure.Attributes;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Posts;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Common;
using InvalidReplayException = WowsKarma.Api.Infrastructure.Exceptions.InvalidReplayException;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class PostController : ControllerBase
	{
		private readonly PlayerService playerService;
		private readonly PostService postService;
		private readonly ILogger<PostController> _logger;

		public PostController(PlayerService playerService, PostService postService, ILogger<PostController> logger)
		{
			this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
			this.postService = postService ?? throw new ArgumentNullException(nameof(postService));
			_logger = logger;
		}

		/// <summary>
		/// Fetches player post with given ID
		/// </summary>
		/// <param name="postId">Post's GUID</param>
		/// <response code="200">Returns <see cref="PlayerPostDTO"/> object of Post with specified ID</response>
		/// <response code="404">No post was found with given ID.</response>
		/// <response code="410">Post is locked by Community Managers.</response>
		[HttpGet("{postId:guid}"), ProducesResponseType(typeof(PlayerPostDTO), 200), ProducesResponseType(404), ProducesResponseType(410)]
		public async Task<IActionResult> GetPostAsync(Guid postId)
			=> await postService.GetPostDTOAsync(postId) is { } post
				? !post.ModLocked || post.Author.Id == User.ToAccountListing().Id || User.IsInRole(ApiRoles.CM)
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
		[HttpGet("{userId}/received"), ProducesResponseType(typeof(IEnumerable<PlayerPostDTO>), 200)]
		public IActionResult GetReceivedPosts(uint userId, [FromQuery] int? lastResults)
		{
			IQueryable<Post> posts = postService.GetReceivedPosts(userId);

			if (User.ToAccountListing()?.Id != userId || !User.IsInRole(ApiRoles.CM))
			{
				posts = posts.Where(p => !p.ModLocked || p.AuthorId == User.ToAccountListing().Id);
			}

			if (lastResults is > 0)
			{
				posts = posts.Take((int)lastResults);
			}

			return Ok(posts.Adapt<List<PlayerPostDTO>>());
		}

		/// <summary>
		/// Fetches all posts that a given player has sent.
		/// </summary>
		/// <param name="userId">Account ID of player to query</param>
		/// <param name="lastResults">Return maximum of results (where available)</param>
		/// <response code="200">List of posts sent by player</response>
		/// <response code="204">No posts sent by given player.</response>
		/// <response code="404">No player found for given Account ID.</response>
		[HttpGet("{userId}/sent"), ProducesResponseType(typeof(IEnumerable<PlayerPostDTO>), 200), ProducesResponseType(204), ProducesResponseType(typeof(string), 404)]
		public IActionResult GetSentPosts(uint userId, [FromQuery] int? lastResults)
		{
			IQueryable<Post> posts = postService.GetSentPosts(userId);

			if (User.ToAccountListing()?.Id != userId || !User.IsInRole(ApiRoles.CM))
			{
				posts = posts?.Where(p => !p.ModLocked);
			}

			if (lastResults is > 0)
			{
				posts = posts.Take((int)lastResults);
			}

			return Ok(posts.Adapt<List<PlayerPostDTO>>());
		}

		/// <summary>
		/// Fetches latest posts.
		/// </summary>
		/// <param name="count">Return maximum of results.</param>
		/// <param name="hasReplay">Filters returned posts by Replay attachment.</param>
		/// <param name="hideModActions">Hides posts containing Mod Actions (visible only to CMs).</param>
		/// <response code="200">List of latest posts, sorted by Submission time.</response>
		[HttpGet("latest"), ProducesResponseType(typeof(IEnumerable<PlayerPostDTO>), 200)]
		public IActionResult GetLatestPosts([FromQuery] int count = 10, bool? hasReplay = null, bool hideModActions = false)
		{
			AccountListingDTO currentUser = User.ToAccountListing();

			IQueryable<Post> posts = postService.GetLatestPosts();

			if (!User.IsInRole(ApiRoles.CM))
			{
				posts = posts.Where(p => !p.ModLocked || p.AuthorId == currentUser.Id);
			}
			else if (hideModActions)
			{
				posts = posts.Where(p => !p.ModLocked);
			}

			if (hasReplay.HasValue)
			{
				posts = hasReplay.Value
					? posts.Where(p => p.Replay != null)
					: posts.Where(p => p.Replay == null);
			}

			return base.StatusCode(200, posts.Take(count).Adapt<List<PlayerPostDTO>>());
		}

		/// <summary>
		/// Submits a new post for creation.
		/// </summary>
		/// <param name="post">Post object to submit</param>
		/// <param name="replay">Optional replay file to attach to post</param>
		/// <param name="ignoreChecks">Bypass API Validation for post creation (Admin only)</param>
		/// <response code="201">Post was successfuly created.</response>
		/// <response code="400">Post contents validation has failed.</response>
		/// <response code="422">Attached replay is invalid.</response>
		/// <response code="403">Restrictions are in effect for one of the targeted accounts.</response>
		/// <response code="404">One of the targeted accounts was not found.</response>
		[HttpPost, Authorize(RequireNoPlatformBans)]
		[ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(422), ProducesResponseType(typeof(string), 403), ProducesResponseType(typeof(string), 404)]
		public async Task<IActionResult> CreatePost(
			[FromForm] string postDto, 
			[FromServices] ReplaysIngestService replaysIngestService,
			IFormFile replay = null, 
			[FromQuery] bool ignoreChecks = false)
		{
            PlayerPostDTO post;
			
            try
			{
				post = JsonSerializer.Deserialize<PlayerPostDTO>(postDto, Common.Utilities.ApiSerializerOptions);
			}
			catch (Exception e)
			{
				return BadRequest(e.ToString());
			}
			
			if (await playerService.GetPlayerAsync(post.Author.Id) is not { } author)
			{
				return StatusCode(404, $"Account {post.Author.Id} not found.");
			}

			if (await playerService.GetPlayerAsync(post.Player.Id) is not { } player)
			{
				return StatusCode(404, $"Account {post.Player.Id} not found.");
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
				if (post.Author.Id != uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
				{
					return StatusCode(403, "Author is not authorized to post on behalf of other users.");
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
				Post created = await postService.CreatePostAsync(post, replay, ignoreChecks);
				return StatusCode(201, created.Id);
			}
			catch (ArgumentException)
			{
				return BadRequest();
			}
			
			catch (InvalidReplayException e) when (e.InnerException is Nodsoft.WowsReplaysUnpack.Core.Exceptions.InvalidReplayException)
			{
				return UnprocessableEntity("Invalid replay.");
			}
			// Handle InvalidReplayException when the Inner exception is a SecurityException and its Data contains an exploit with value "CVE-2022-31265".
			catch (InvalidReplayException e) when (e.InnerException is SecurityException se && se.Data["exploit"] is "CVE-2022-31265")
			{
				// Log this exception, and store the replay with the RCE the samples.
				_logger.LogWarning(se, "Replay upload failed for post author {author} due to CVE-2022-31265 exploit detection.", post.Author.Id);
				await replaysIngestService.IngestRceFileAsync(replay);

				throw se;
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
		[HttpPut, Authorize(RequireNoPlatformBans), ETag(false)]
		[ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(typeof(string), 403), ProducesResponseType(typeof(string), 404)]
		public async Task<IActionResult> EditPost([FromBody] PlayerPostDTO post, [FromQuery] bool ignoreChecks = false)
		{
			if (postService.GetPost(post.Id ?? Guid.Empty) is not { } current)
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
				
				if (current is { ModLocked: true } or { ReadOnly: true })
				{
					return StatusCode(403, "Post has been locked by Community Managers. No modification is possible.");
				}
			}

			try
			{
				await postService.EditPostAsync(post.Id ?? Guid.Empty, post);
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
		[HttpDelete("{postId:guid}"), Authorize(RequireNoPlatformBans)]
		[ProducesResponseType(205), ProducesResponseType(typeof(string), 403), ProducesResponseType(typeof(string), 404)]
		public async Task<IActionResult> DeletePost(Guid postId, [FromQuery] bool ignoreChecks = false)
		{
			if (postService.GetPost(postId) is not { } post)
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
	}
}
