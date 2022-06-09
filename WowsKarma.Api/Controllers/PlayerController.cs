using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Mapster;
using WowsKarma.Api.Services;
using WowsKarma.Common;


namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class PlayerController : ControllerBase
	{
		private readonly PlayerService service;

		public PlayerController(PlayerService playerService)
		{
			service = playerService;
		}

		/// <summary>
		/// Lists accounts containing usernames starting with given search query.
		/// (Max. 100 results)
		/// </summary>
		/// <param name="query">Username search query</param>
		/// <response code="200">Account listings for given search query</response>
		/// <response code="204">No results found for given search query</response>
		[HttpGet("search/{query}"), ProducesResponseType(typeof(IEnumerable<AccountListingDTO>), 200), ProducesResponseType(204)]
		public async Task<IActionResult> SearchAccount([StringLength(100, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9_]*$")] string query)
		{
			IEnumerable<AccountListingDTO> accounts = await service.ListPlayersAsync(query);

			return accounts is null
				? StatusCode(204)
				: StatusCode(200, accounts);
		}

		/// <summary>
		/// Fetches the player profile for a given Account ID.
		/// </summary>
		/// <param name="id">Player account ID</param>
		/// <param name="includeClanInfo">Include clan membership info while fetching player profile.</param>
		/// <response code="200">Returns player profile</response>
		/// <response code="204">No profile found</response>
		[HttpGet("{id}"), ProducesResponseType(typeof(PlayerProfileDTO), 200), ProducesResponseType(204)]
		public async Task<IActionResult> GetAccount(uint id, bool includeClanInfo = true)
		{
			if (id is 0)
			{
				return StatusCode(400, new ArgumentException(null, nameof(id)));
			}

			Player playerProfile = await service.GetPlayerAsync(id, false, includeClanInfo);

			return playerProfile is null
				? StatusCode(204)
				: StatusCode(200, playerProfile.Adapt<PlayerProfileDTO>());
		}

		/// <summary>
		/// Fetches Site Karma for each provided Account ID, where available.
		/// </summary>
		/// <param name="ids">List of Account IDs</param>
		/// <response code="200">Returns "Account":"SiteKarma" Dictionary of Karma metrics for available accounts (may be empty).</response>
		[HttpPost("karmas"), ProducesResponseType(typeof(Dictionary<uint, int>), 200)]
		public IActionResult FetchKarmas([FromBody] uint[] ids) => StatusCode(200, AccountKarmaDTO.ToDictionary(service.GetPlayersKarma(ids)));

		/// <summary>
		/// Fetches full Karma metrics (Site Karma and Flairs) for each provided Account ID, where available.
		/// </summary>
		/// <param name="ids">List of Account IDs</param>
		/// <response code="200">Returns Full Karma metrics for available accounts (may be empty).</response>
		[HttpPost("karmas-full"), ProducesResponseType(typeof(IEnumerable<AccountFullKarmaDTO>), 200)]
		public IActionResult FetchFullKarmas([FromBody] uint[] ids) => StatusCode(200, service.GetPlayersFullKarma(ids));

		/// <summary>
		/// Triggers recalculation of Karma metrics for a given account.
		/// </summary>
		/// <remarks>
		/// Can only be called by Site Administrators.
		/// </remarks>
		/// <param name="playerId">Account ID of player profile</param>
		/// <param name="cancellationToken"></param>
		/// <response code="205">Profile Karma recalculation was processed.</response>
		[HttpPost("recalculate"), Authorize(Roles = ApiRoles.Administrator), ProducesResponseType(205), ProducesResponseType(401), ProducesResponseType(403)]
		public async Task<IActionResult> RecalculateMetrics([FromQuery] uint playerId, CancellationToken cancellationToken)
		{
			await service.RecalculatePlayerMetrics(playerId, cancellationToken);
			return StatusCode(205);
		}
	}
}
