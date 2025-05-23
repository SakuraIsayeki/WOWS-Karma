using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Hangfire;
using Mapster;
using WowsKarma.Api.Services;
using WowsKarma.Common;

namespace WowsKarma.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class PlayerController : ControllerBase
{
	private readonly PlayerService _playerService;

	public PlayerController(PlayerService playerService)
	{
		_playerService = playerService;
	}

	/// <summary>
	/// Lists all players in the database.
	/// </summary>
	/// <returns>A list of all players in the database.</returns>
	/// <response code="200">Returns all players in the database.</response>
	[HttpGet]
	public IAsyncEnumerable<uint> ListPlayers() => _playerService.ListPlayerIds();

	/// <summary>
	/// Lists accounts containing usernames starting with given search query.
	/// (Max. 100 results)
	/// </summary>
	/// <param name="query">Username search query</param>
	/// <response code="200">Account listings for given search query</response>
	/// <response code="204">No results found for given search query</response>
	[HttpGet("search/{query}"), ProducesResponseType(typeof(IEnumerable<AccountListingDTO>), 200), ProducesResponseType(204)]
	public async Task<ActionResult<AccountListingDTO>> SearchAccount([StringLength(100, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9_]*$")] string query) 
		=> await _playerService.ListPlayersAsync(query) is { Length: not 0 } accounts
			? Ok(accounts)
			: NoContent();

	/// <summary>
	/// Fetches the player profile for a given Account ID.
	/// </summary>
	/// <param name="id">Player account ID</param>
	/// <param name="includeClanInfo">Include clan membership info while fetching player profile.</param>
	/// <response code="200">Returns player profile</response>
	/// <response code="204">No profile found</response>
	[HttpGet("{id}")]
	public async Task<ActionResult<PlayerProfileDTO>> GetAccount(uint id, bool includeClanInfo = true)
	{
		if (id is 0)
		{
			ModelState.AddModelError(nameof(id), "Account ID cannot be zero.");
			return BadRequest(ModelState);
		}

		Player? playerProfile = await _playerService.GetPlayerAsync(id, false, includeClanInfo);

		return playerProfile is null
			? NotFound()
			: Ok(playerProfile.Adapt<PlayerProfileDTO>());
	}

	/// <summary>
	/// Fetches Site Karma for each provided Account ID, where available.
	/// </summary>
	/// <param name="ids">List of Account IDs</param>
	/// <response code="200">Returns "Account":"SiteKarma" Dictionary of Karma metrics for available accounts (may be empty).</response>
	[HttpPost("karmas"), ProducesResponseType(typeof(Dictionary<uint, int>), 200)]
	public Dictionary<uint, int> FetchKarmas([FromBody] uint[] ids) => AccountKarmaDTO.ToDictionary(_playerService.GetPlayersKarma(ids));

	/// <summary>
	/// Fetches full Karma metrics (Site Karma and Flairs) for each provided Account ID, where available.
	/// </summary>
	/// <param name="ids">List of Account IDs</param>
	/// <response code="200">Returns Full Karma metrics for available accounts (may be empty).</response>
	[HttpPost("karmas-full")]
	public IEnumerable<AccountFullKarmaDTO> FetchFullKarmas([FromBody] uint[] ids) => _playerService.GetPlayersFullKarma(ids);

	/// <summary>
	/// Triggers recalculation of Karma metrics for a given account.
	/// </summary>
	/// <remarks>
	/// Can only be called by Site Administrators.
	/// </remarks>
	/// <param name="playerId">Account ID of player profile</param>
	/// <param name="ct"></param>
	/// <response code="205">Profile Karma recalculation was processed.</response>
	/// <response code="401">Unauthorized</response>
	/// <response code="403">Forbidden</response>
	[HttpPatch("recalculate"), Authorize(Roles = ApiRoles.Administrator)]
	public AcceptedResult RecalculateMetrics([FromQuery] uint playerId, CancellationToken ct)
	{
		BackgroundJob.Enqueue<PlayerService>(p => p.RecalculatePlayerMetrics(playerId, ct));
		return Accepted();
	}
}