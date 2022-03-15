using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Infrastructure.Exceptions;
using WowsKarma.Api.Services;
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

	public ReplayController(ReplaysIngestService ingestService, ReplaysProcessService processService, PostService postService)
	{
		_ingestService = ingestService;
		_processService = processService;
		_postService = postService;
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
			return NotFound($"No post with GUID {postId} found.");
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
	}

	/// <summary>
	/// Triggers reprocessing on a replay (Usable only by Administrators)
	/// </summary>
	/// <returns></returns>
	[HttpPatch("reprocess"), Authorize(Roles = ApiRoles.Administrator)]
	public async Task<IActionResult> ReprocessPostsAsync(CancellationToken ct)
	{
		await _ingestService.ReprocessAllReplaysAsync(ct);
		return StatusCode(200);
	}
}
