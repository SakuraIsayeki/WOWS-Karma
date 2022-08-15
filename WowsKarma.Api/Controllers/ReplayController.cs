using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
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
public class ReplayController : ControllerBase
{
	private readonly ReplaysIngestService _ingestService;
	private readonly ReplaysProcessService _processService;
	private readonly PostService _postService;
	private readonly ILogger<ReplayController> _logger;

	public ReplayController(ReplaysIngestService ingestService, ReplaysProcessService processService, PostService postService, ILogger<ReplayController> logger)
	{
		_ingestService = ingestService;
		_processService = processService;
		_postService = postService;
		_logger = logger;
	}

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
	/// <returns></returns>
	[HttpPatch("reprocess/all"), Authorize(Roles = ApiRoles.Administrator)]
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
		
		await _ingestService.ReprocessAllReplaysAsync(start.ToUniversalTime(), end.ToUniversalTime(), ct);
		return StatusCode(200);
	}
	
	/// <summary>
	/// Triggers reporessing on a replay (Usable only by Administrators)
	/// </summary>
	/// <returns></returns>
	[HttpPatch("reprocess/{replayId:guid}"), Authorize(Roles = ApiRoles.Administrator)]
	public async Task<IActionResult> ReprocessReplayAsync(Guid replayId, CancellationToken ct = default)
	{
		try
		{
			await _ingestService.ReprocessReplayAsync(replayId, ct);
			return StatusCode(200);
		}
		catch (ArgumentException)
		{
			return StatusCode(404, $"No replay with GUID {replayId} found.");
		}
	}
}
