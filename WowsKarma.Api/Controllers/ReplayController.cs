using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Services.Replays;
using WowsKarma.Common.Models.DTOs.Replays;

namespace WowsKarma.Api.Controllers;


[ApiController, Route("api/[controller]")]
public class ReplayController : ControllerBase
{
	private readonly ReplaysIngestService _ingestService;
	private readonly ReplaysProcessService _processService;

	public ReplayController(ReplaysIngestService ingestService, ReplaysProcessService processService)
	{
		_ingestService = ingestService;
		_processService = processService;
	}

	[HttpGet("{replayId}")]
	public Task<ReplayDTO> GetAsync(Guid replayId) => _ingestService.GetReplayDTOAsync(replayId);

	[HttpPost("{postId}"), RequestSizeLimit(ReplaysIngestService.MaxReplaySize)]
	public async Task<IActionResult> UploadReplayAsync(Guid postId, IFormFile replay, CancellationToken ct)
	{
		Replay ingested = await _ingestService.IngestReplayAsync(postId, replay, ct);
		return base.Ok(await _processService.ProcessReplayAsync(ingested.Id, replay.OpenReadStream(), ct));
	}
}
