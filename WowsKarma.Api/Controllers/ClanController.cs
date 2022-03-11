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

	[HttpGet("{clanId}")]
	public async Task<ClanProfileDTO> GetClan(uint clanId, bool includeMembers = true, CancellationToken ct = default)
	{
		Clan clan = await _clanService.GetClanAsync(clanId, includeMembers, ct);

		return includeMembers 
			? clan.Adapt<ClanProfileFullDTO>()
			: clan.Adapt<ClanProfileDTO>();
	}

	[HttpGet("search/{search:length(2,50)}")]
	public async Task<IEnumerable<ClanListingDTO>> SearchClans(string search, [Range(0, 500)] uint results = 20) => await _clanService.SearchClansAsync(search, results);
}