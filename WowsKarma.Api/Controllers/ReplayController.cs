using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using WowsKarma.Api.Data.Models.Replays;
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

	[HttpGet("{replayId}")]
	public Task<ReplayDTO> GetAsync(Guid replayId) => _ingestService.GetReplayDTOAsync(replayId);

	[HttpPost("{postId}"), Authorize, RequestSizeLimit(ReplaysIngestService.MaxReplaySize)]
	public async Task<IActionResult> UploadReplayAsync(Guid postId, IFormFile replay, CancellationToken ct, [FromQuery] bool ignoreChecks = false)
	{
		if (_postService.GetPost(postId) is not Post current)
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



		Replay ingested = await _ingestService.IngestReplayAsync(postId, replay, ct);
		await _processService.ProcessReplayAsync(ingested.Id, replay.OpenReadStream(), ct);

		return Ok(_ingestService.GetReplayDTOAsync(ingested.Id));
	}
}
