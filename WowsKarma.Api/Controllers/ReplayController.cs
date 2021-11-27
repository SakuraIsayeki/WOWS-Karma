using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using WowsKarma.Api.Services.Replays;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class ReplayController : ControllerBase
	{
		private readonly ReplaysIngestService _service;

		public ReplayController(ReplaysIngestService service)
		{
			_service = service;
		}


		[HttpPost, RequestSizeLimit(ReplaysIngestService.MaxReplaySize)]
		public async Task<IActionResult> UploadReplayAsync(IFormFile replay, CancellationToken ct)
		{

			return Ok(await _service.UploadReplayFileAsync(replay, ct));
		}
	}
}
