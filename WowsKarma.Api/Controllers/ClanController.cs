using System.Threading;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using WowsKarma.Api.Services;

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
	public async Task<IActionResult> GetClan(uint clanId, bool includeMembers, CancellationToken ct = default)
	{
		Clan clan = await _clanService.GetClanAsync(clanId, includeMembers, ct);

		return Ok(includeMembers 
			? clan.Adapt<ClanFullDTO>()
			: clan.Adapt<ClanDTO>());
	}
	
}