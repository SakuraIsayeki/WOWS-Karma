using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using WowsKarma.Api.Data.Models.Replays;
using WowsKarma.Api.Services.Replays;

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


	[HttpPost("{postId}"), RequestSizeLimit(ReplaysIngestService.MaxReplaySize)]
	public async Task<IActionResult> UploadReplayAsync([FromRoute] Guid postId, IFormFile replay, CancellationToken ct)
	{
		Replay ingested = await _ingestService.IngestReplayAsync(postId, replay, ct);
		return base.Ok(await _processService.ProcessReplayAsync(ingested.Id, replay.OpenReadStream(), ct));
	}
}
