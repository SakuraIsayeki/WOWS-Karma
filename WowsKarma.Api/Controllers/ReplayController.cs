using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using Hangfire;
using Microsoft.Extensions.Logging;
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
	public Task<ReplayDTO> GetAsync(Guid replayId) => _ingestService.GetReplayDTOAsync(replayId);

	[HttpPost("{postId:guid}"), Authorize, RequestSizeLimit(ReplaysIngestService.MaxReplaySize), ProducesResponseType(201)]
	public async Task<IActionResult> UploadReplayAsync(Guid postId, IFormFile replay, CancellationToken ct, [FromQuery] bool ignoreChecks = false)
	{
		if (_postService.GetPost(postId) is not { } current)
		{
			return StatusCode(404, $"No post with GUID {postId} found.");
		}

		if (ignoreChecks)
		{
			if (!(User.IsInRole(ApiRoles.CM) || User.IsInRole(ApiRoles.Administrator)))
			{
				return StatusCode(403, "Post Author is not authorized to bypass Replay checks.");
			}
		}
		else
		{
			if (current.AuthorId != uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
			{
				return StatusCode(403, "Post Author is not authorized to edit post replays on behalf of other users.");
			}
			if (current.ModLocked)
			{
				return StatusCode(403, "Specified Post has been locked by Community Managers. No modification is possible.");
			}
		}


		try
		{
			Replay ingested = await _ingestService.IngestReplayAsync(postId, replay, ct);
			return StatusCode(201, _ingestService.GetReplayDTOAsync(ingested.Id));
		}
		catch (InvalidReplayException e)
		{
			return BadRequest(e);
		}
		// Handle security exception, related to CVE-2022-31265.
		catch (SecurityException se) when (se.Data["exploit"] is "CVE-2022-31265")
		{
			// Log this exception, and store the replay with the RCE the samples.
			_logger.LogWarning(se, "Replay upload failed due to CVE-2022-31265 exploit detection.");
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
	public async Task<IActionResult> ReprocessPostsAsync(DateTime start = default, DateTime end = default, CancellationToken ct = default)
	{
		if (start == default)
		{
			start = DateTime.UnixEpoch;
		}

		if (end == default)
		{
			end = DateTime.UtcNow;
		}
		
		BackgroundJob.Enqueue<ReplaysIngestService>(s => s.ReprocessAllReplaysAsync(start.ToUniversalTime(), end.ToUniversalTime(), ct));
		return StatusCode(202);
	}
	
	/// <summary>
	/// Triggers reporessing on a replay (Usable only by Administrators)
	/// </summary>
	/// <returns></returns>
	[HttpPatch("reprocess/replay/{replayId:guid}"), Authorize(Roles = ApiRoles.Administrator)]
	public async Task<IActionResult> ReprocessReplayAsync(Guid replayId, CancellationToken ct = default)
	{
		try
		{
			BackgroundJob.Enqueue<ReplaysIngestService>(s => s.ReprocessReplayAsync(replayId, ct));
			return StatusCode(202);
		}
		catch (ArgumentException)
		{
			return StatusCode(404, $"No replay with GUID {replayId} found.");
		}
	}

	/// <summary>
	/// Triggers minimap rendering on a post's replay (Usable only by Administrators)
	/// </summary>
	/// <param name="postId">The ID of the post to render the replay's minimap for.</param>
	/// <param name="postService"></param>
	/// <param name="minimapRenderingService"></param>
	/// <param name="force">Whether to force rendering the minimap, even if it has already been rendered.</param>
	/// <param name="waitForCompletion">Whether to wait for the job to complete before returning.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <response code="200">The minimap was rendered successfully.</response>
	/// <response code="202">The job was enqueued successfully.</response>
	/// <response code="404">No post with the specified GUID was found.</response>
	[HttpPatch("reprocess/minimap/{postId:guid}"), Authorize(Roles = ApiRoles.Administrator)]
	public async ValueTask<IActionResult> RenderMinimap(Guid postId,
		[FromServices] MinimapRenderingService minimapRenderingService,
		[FromQuery] bool force = false,
		[FromQuery] bool waitForCompletion = false,
		CancellationToken ct = default
	) {
		if (_postService.GetPost(postId) is not { } post)
		{
			return StatusCode(404, $"No post with GUID {postId} found.");
		}

		if (waitForCompletion)
		{
			await minimapRenderingService.RenderPostReplayMinimapAsync(post.Id, force, ct);
		}
		else
		{
			BackgroundJob.Enqueue<MinimapRenderingService>(s => s.RenderPostReplayMinimapAsync(post.Id, force, ct));
		}
        
		return StatusCode(waitForCompletion ? 200 : 202);
	}
	
	/// <summary>
	/// Triggers minimap rendering on all posts' replays within the specified date/time range (Usable only by Administrators)
	/// </summary>
	/// <param name="start">Start of date/time range</param>
	/// <param name="end">End of date/time range</param>
	/// <param name="force">Whether to force rendering a minimap, even if it has already been rendered.</param>
	/// <param name="ct">Cancellation token</param>
	/// <response code="202">The job was enqueued successfully.</response>
	[HttpPatch("reprocess/minimap/all"), Authorize(Roles = ApiRoles.Administrator)]
	public async Task<IActionResult> RenderMinimapsAsync(DateTime start = default, DateTime end = default, bool force = false, CancellationToken ct = default)
	{
		if (start == default)
		{
			start = DateTime.UnixEpoch;
		}

		if (end == default)
		{
			end = DateTime.UtcNow;
		}
		
		BackgroundJob.Enqueue<MinimapRenderingService>(s => s.ReprocessAllMinimapsAsync(start.ToUniversalTime(), end.ToUniversalTime(), force, ct));
		return StatusCode(202);
	}
}
