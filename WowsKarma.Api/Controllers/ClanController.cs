using System.ComponentModel.DataAnnotations;
using System.Threading;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using WowsKarma.Api.Services;
using WowsKarma.Common.Models.DTOs.Clans;

namespace WowsKarma.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class ClanController : ControllerBase
{
	private readonly ClanService _clanService;

	public ClanController(ClanService clanService)
	{
		_clanService = clanService;
	}

	/// <summary>
	/// Fetches Clan info for a given Clan ID, along with clan members (unless excluded).
	/// </summary>
	/// <param name="clanId">ID of Clan to fetch info/members for</param>
	/// <param name="includeMembers">Select whether response must include members</param>
	/// <param name="ct"></param>
	/// <returns>Clan Info, with members (if selected)</returns>
	[HttpGet("{clanId}"), ProducesResponseType(typeof(ClanProfileDTO), 200), ProducesResponseType(typeof(ClanProfileFullDTO), 200)]
	public async Task<ClanProfileDTO> GetClan(uint clanId, bool includeMembers = true, CancellationToken ct = default)
	{
		Clan clan = await _clanService.GetClanAsync(clanId, includeMembers, ct);

		return includeMembers 
			? clan.Adapt<ClanProfileFullDTO>()
			: clan.Adapt<ClanProfileDTO>();
	}

	/// <summary>
	/// Searches all clans relevant to a given search string.
	/// </summary>
	/// <param name="search">Search query (Clan Tag and/or Name)</param>
	/// <param name="results">Amount of maximum results to return</param>
	/// <returns>List of clans matching given search string</returns>
	[HttpGet("search/{search}"), ProducesResponseType(typeof(IEnumerable<ClanListingDTO>), 200)]
	public async Task<IEnumerable<ClanListingDTO>> SearchClans([FromRoute, StringLength(50, MinimumLength = 2)] string search, [FromQuery, Range(0, 500)] uint results = 20) 
		=> await _clanService.SearchClansAsync(search, results);
}