using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hangfire;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Posts;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Common;
using WowsKarma.Common.Models.DTOs.Replays;

namespace WowsKarma.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ReplayController : ControllerBase
{
	private readonly ReplaysIngestService _ingestService;
	private readonly ReplaysProcessService _processService;
	private readonly PostService _postService;
	private readonly ILogger<ReplayController> _logger;

	public ReplayController(
		ReplaysIngestService ingestService, 
		ReplaysProcessService processService, 
		PostService postService, 
		ILogger<ReplayController> logger
	) {
		_ingestService = ingestService;
		_processService = processService;
		_postService = postService;
		_logger = logger;
	}

	/// <summary>
	/// Lists all replays by ID.
	/// </summary>
	/// <returns>List of all replays IDs.</returns>
	/// <response code="200">Returns list of all replays IDs.</response>
	[HttpGet, ProducesResponseType(StatusCodes.Status200OK)]
	public IAsyncEnumerable<Guid> List() => _ingestService.ListReplaysAsync();

	/// <summary>
	/// Gets Replay data for given Replay ID
	/// </summary>
	/// <param name="replayId">ID of Replay to fetch</param>
	/// <returns>Replay data</returns>
	[HttpGet("{replayId:guid}"), ProducesResponseType(typeof(ReplayDTO), 200)]
	public Task<ReplayDTO?> GetAsync(Guid replayId) => _ingestService.GetReplayDTOAsync(replayId);

	[HttpPost("{postId:guid}"), Authorize, RequestSizeLimit(ReplaysIngestService.MaxReplaySize)]
	public async Task<ActionResult> UploadReplayAsync(Guid postId, IFormFile replay, CancellationToken ct, [FromQuery] bool ignoreChecks = false)
	{
		if (_postService.GetPost(postId) is not { } current)
		{
			ModelState.AddModelError("postId", $"No post with GUID {postId} found.");
			return BadRequest(ModelState);
		}

		if (ignoreChecks)
		{
			if (!(User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator)))
			{
				ModelState.AddModelError("ignoreChecks", "Only Community Managers and Administrators are allowed to bypass Replay checks.");
				return BadRequest(ModelState);
			}
		}
		else
		{
			if (current.AuthorId != uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing NameIdentifier claim.")))
			{
				ModelState.AddModelError("postId", "Post Author is not authorized to upload replays on behalf of other users.");
				return BadRequest(ModelState);
			}
			if (current.ModLocked)
			{
				ModelState.AddModelError("postId", "Specified Post has been locked by Community Managers. No modification is possible.");
				return BadRequest(ModelState);
			}
		}

		try
		{
			Replay ingested = await _ingestService.IngestReplayAsync(postId, replay, ct);
			return CreatedAtAction("Get", new { replayId = ingested.Id }, _ingestService.GetReplayDTOAsync(ingested.Id));
		}
		catch (InvalidReplayException e)
		{
			return BadRequest(e);
		}
		// Handle security exception, related to CVE-2022-31265.
		catch (SecurityException se) when (se.Data["exploit"] is "CVE-2022-31265")
		{
			// Log this exception, and store the replay with the RCE the samples.
			_logger.LogWarning(se, "Replay upload failed due to CVE-2022-31265 exploit detection");
			await _ingestService.IngestRceFileAsync(replay);
			return BadRequest(se);
		}
	}

	/// <summary>
	/// Triggers reprocessing on all replays within the specified date/time range (Usable only by Administrators)
	/// </summary>
	/// <param name="start">Start of date/time range</param>
	/// <param name="end">End of date/time range</param>
	[HttpPatch("reprocess/replay/all"), Authorize(Roles = ApiRoles.Administrator)]
	public AcceptedResult ReprocessPosts(DateTime start = default, DateTime end = default)
	{
		if (start == default)
		{
			start = DateTime.UnixEpoch;
		}

		if (end == default)
		{
			end = DateTime.UtcNow;
		}
		
		BackgroundJob.Enqueue<ReplaysIngestService>(s => s.ReprocessAllReplaysAsync(start.ToUniversalTime(), end.ToUniversalTime(), HttpContext.RequestAborted));
		return Accepted();
	}
	
	/// <summary>
	/// Triggers reporessing on a replay (Usable only by Administrators)
	/// </summary>
	[HttpPatch("reprocess/replay/{replayId:guid}"), Authorize(Roles = ApiRoles.Administrator)]
	public IActionResult ReprocessReplay(Guid replayId)
	{
		try
		{
			BackgroundJob.Enqueue<ReplaysIngestService>(s => s.ReprocessReplayAsync(replayId, HttpContext.RequestAborted));
			return AcceptedAtAction("Get", routeValues: new { replayId }, null);
		}
		catch (ArgumentException)
		{
			return NotFound();
		}
	}

	/// <summary>
	/// Triggers minimap rendering on a post's replay (Usable only by Administrators)
	/// </summary>
	/// <param name="postId">The ID of the post to render the replay's minimap for.</param>
	/// <param name="minimapRenderingService"></param>
	/// <param name="force">Whether to force rendering the minimap, even if it has already been rendered.</param>
	/// <param name="waitForCompletion">Whether to wait for the job to complete before returning.</param>
	/// <response code="200">The minimap was rendered successfully.</response>
	/// <response code="202">The job was enqueued successfully.</response>
	/// <response code="404">No post with the specified GUID was found.</response>
	[HttpPatch("reprocess/minimap/{postId:guid}"), Authorize(Roles = ApiRoles.Administrator)]
	public async ValueTask<ActionResult> RenderMinimap(Guid postId,
		[FromServices] MinimapRenderingService minimapRenderingService,
		[FromQuery] bool force = false,
		[FromQuery] bool waitForCompletion = false
	) {
		if (_postService.GetPost(postId) is not { } post)
		{
			return NotFound();
		}

		if (waitForCompletion)
		{
			await minimapRenderingService.RenderPostReplayMinimapAsync(post.Id, force, HttpContext.RequestAborted);
			return Ok();
		}
		else
		{
			BackgroundJob.Enqueue<MinimapRenderingService>(s => s.RenderPostReplayMinimapAsync(post.Id, force, HttpContext.RequestAborted));
			return AcceptedAtAction("Get", routeValues: new { postId }, null);
		}
	}
	
	/// <summary>
	/// Triggers minimap rendering on all posts' replays within the specified date/time range (Usable only by Administrators)
	/// </summary>
	/// <param name="start">Start of date/time range</param>
	/// <param name="end">End of date/time range</param>
	/// <param name="force">Whether to force rendering a minimap, even if it has already been rendered.</param>
	/// <response code="202">The job was enqueued successfully.</response>
	[HttpPatch("reprocess/minimap/all"), Authorize(Roles = ApiRoles.Administrator)]
	public AcceptedResult RenderMinimaps(DateTime start = default, DateTime end = default, bool force = false)
	{
		if (start == default)
		{
			start = DateTime.UnixEpoch;
		}

		if (end == default)
		{
			end = DateTime.UtcNow;
		}
		
		BackgroundJob.Enqueue<MinimapRenderingService>(s => s.ReprocessAllMinimapsAsync(start.ToUniversalTime(), end.ToUniversalTime(), force, HttpContext.RequestAborted));
		return Accepted();
	}
}
