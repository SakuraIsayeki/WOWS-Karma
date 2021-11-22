using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WowsKarma.Api.Data;
using WowsKarma.Api.Services;
using WowsKarma.Common;


namespace WowsKarma.Api.Controllers.Admin;


[ApiController, Route("api/mod/action"), Authorize(Roles = ApiRoles.CM)]
public class ModActionController : ControllerBase
{
	private readonly ModService _service;

	public ModActionController(ModService service)
	{
		_service = service;
	}

	/// <summary>
	/// Fetches ModAction by ID.
	/// </summary>
	/// <param name="id">ID of ModAction to fetch.</param>
	[HttpGet("{id}"), AllowAnonymous]
	public async Task<PostModActionDTO> Fetch(Guid id) => (await _service.GetModActionAsync(id)).Adapt<PostModActionDTO>();

	/// <summary>
	/// Lists ModActions by Post or User IDs.
	/// </summary>
	/// <remarks>
	/// User ID gets ignored if a Post ID is provided.
	/// </remarks>
	/// <param name="postId">Get ModActions for specific Post.</param>
	/// <param name="userId">Get ModActions for specific User.</param>
	/// <response code="200">Returns list of ModActions.</response>
	/// <response code="204">No ModActions found.</response>
	[HttpGet("list"), AllowAnonymous, ProducesResponseType(typeof(IEnumerable<PostModActionDTO>), 200), ProducesResponseType(204)]
	public IActionResult List([FromQuery] Guid postId = default, [FromQuery] uint userId = default)
	{
		IEnumerable<PostModAction> modActions;

		if (postId != default)
		{
			modActions = _service.GetPostModActions(postId).ToArray();
		}
		else if (userId is not 0)
		{
			modActions = _service.GetPostModActions(userId).ToArray();
		}
		else
		{
			return BadRequest("Please use a search query (Post/User).");
		}

		return modActions?.Count() is null or 0
			? base.StatusCode(204)
			: base.StatusCode(200, modActions.Adapt<IEnumerable<PostModActionDTO>>());
	}

	/// <summary>
	/// Submits a new ModAction.
	/// </summary>
	/// <remarks>
	/// Usable only by Community Managers.
	/// </remarks>
	/// <param name="modAction">ModAction to submit</param>
	/// <response code="202">ModAction was successfully submitted.</response>
	[HttpPost, Authorize(Roles = ApiRoles.CM), ProducesResponseType(202)]
	public async Task<IActionResult> Submit([FromBody] PostModActionDTO modAction)
	{
		uint modId = uint.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
		await _service.SubmitModActionAsync(modAction with { ModId = modId });
		return StatusCode(202);
	}

	/// <summary>
	/// Deletes an existing ModAction.
	/// </summary>
	/// <remarks>
	/// Usable only by Community Managers.
	/// </remarks>
	/// <param name="id">ID of ModAction to delete.</param>
	/// <response code="205">ModAction was flagged for deletion.</response>
	[HttpDelete("{id}"), Authorize(Roles = ApiRoles.CM), ProducesResponseType(205)]
	public async Task<IActionResult> Revert(Guid id)
	{
		await _service.RevertModActionAsync(id);
		return StatusCode(205);
	}


	#region Platform Bans

	/// <summary>
	/// Fetches all bans emitted for a specific user.
	/// </summary>
	/// <param name="userId">ID of user account</param>
	/// <response code="200">Returns list of Platform Bans.</response>
	/// <response code="204">No Platform Bans found for user.</response>
	[HttpGet("bans/{userId}"), ProducesResponseType(typeof(IEnumerable<PlatformBanDTO>), 200)]
	public IActionResult FetchBans(uint userId) => StatusCode(200, _service.GetPlatformBans(userId).ToList());

	/// <summary>
	///	Emits a new Platform Ban.
	/// </summary>
	/// <param name="submitted">Platform Ban to emit</param>
	/// <param name="days">(Helper) Sets a temporary ban, to the number of specified days starting from UTC now.</param>
	/// <returns></returns>
	[HttpPost("bans"), ProducesResponseType(202)]
	public async Task<IActionResult> SubmitBan([FromBody] PlatformBanDTO submitted, [FromServices] AuthDbContext authDb, [FromQuery] uint days = 0)
	{
		await _service.EmitPlatformBanAsync(submitted with
		{
			ModId = User.ToAccountListing().Id,
			Reverted = false,
			BannedUntil = days is 0 ? null : DateTime.UtcNow.AddDays(days)
		}, authDb);

		return StatusCode(202);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[HttpDelete("bans")]
	public async Task<IActionResult> RevertBan([FromQuery] Guid id)
	{
		return StatusCode(501);
	}

	#endregion // Platform Bans
}
