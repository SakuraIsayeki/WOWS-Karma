using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WowsKarma.Api.Data;
using WowsKarma.Api.Services;
using WowsKarma.Common;

namespace WowsKarma.Api.Controllers.Admin;

[ApiController, Route("api/mod/bans"), Authorize(Roles = $"{ApiRoles.CM},{ApiRoles.Administrator}")]
public sealed class PlatformBansController : ControllerBase
{
	private readonly ModService _service;

	public PlatformBansController(ModService service)
	{
		_service = service;
	}


	/// <summary>
	/// Fetches all bans emitted for a specific user.
	/// </summary>
	/// <param name="userId">ID of user account</param>
	/// <param name="currentOnly">Return only currently active platform bans.</param>
	/// <response code="200">Returns list of Platform Bans.</response>
	/// <response code="204">No Platform Bans found for user.</response>
	[HttpGet("{userId}"), AllowAnonymous, ProducesResponseType(typeof(IEnumerable<PlatformBanDTO>), 200)]
	public IActionResult FetchBans(uint userId, bool currentOnly)
	{
		IQueryable<PlatformBan> bans = _service.GetPlatformBans(userId);

		if (currentOnly || User.ToAccountListing()!.Id != userId || !User.IsInRole(ApiRoles.CM))
		{
			bans = bans.Where(b => b.BannedUntil == null || b.BannedUntil > DateTime.UtcNow);
		}

		return Ok(bans.ProjectToType<PlatformBanDTO>().AsAsyncEnumerable());
	}

	///  <summary>
	/// 	Emits a new Platform Ban.
	///  </summary>
	///  <param name="submitted">Platform Ban to emit</param>
	///  <param name="authDb">(DI)</param>
	///  <param name="days">(Helper) Sets a temporary ban, to the number of specified days starting from UTC now.</param>
	///  <response code="202">Platform Ban was successfuly submitted.</response>
	[HttpPost, ProducesResponseType(202)]
	public async Task<IActionResult> SubmitBan([FromBody] PlatformBanDTO submitted, [FromServices] AuthDbContext authDb, [FromQuery] uint days = 0)
	{
		await _service.EmitPlatformBanAsync(submitted with
		{
			ModId = User.ToAccountListing()!.Id,
			Reverted = false,
			BannedUntil = days is 0 ? null : DateTimeOffset.UtcNow.AddDays(days)
		}, authDb);

		return Accepted();
	}

	/// <summary>
	/// Reverts Platform Ban with specified ID.
	/// </summary>
	/// <param name="id">ID of Platform Ban to revert.</param>
	/// <response code="200">Platform Ban was successfully reverted.</response>
	[HttpDelete("{id:guid}"), ProducesResponseType(200)]
	public async Task<IActionResult> RevertBan(Guid id)
	{
		await _service.RevertPlatformBanAsync(id);

		return StatusCode(205);
	}
}
