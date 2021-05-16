using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Services;
using WowsKarma.Api.Services.Authentication;
using WowsKarma.Common.Models.DTOs;



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

		[HttpGet("Search/{query}")]
		public async Task<IActionResult> SearchAccount(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return StatusCode(400, new ArgumentNullException(nameof(query)));
			}

			IEnumerable<AccountListingDTO> accounts = await service.ListPlayersAsync(query);

			return accounts is null
				? StatusCode(204) 
				: StatusCode(200, accounts);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetAccount(uint id)
		{
			if (id is 0)
			{
				return StatusCode(400, new ArgumentException(null, nameof(id)));
			}

			PlayerProfileDTO playerProfile = await service.GetPlayerAsync(id);

			return playerProfile is null
				? StatusCode(204)
				: StatusCode(200, playerProfile);
		}

		[HttpGet("{id}/metaflags")]
		public async Task<IActionResult> GetMetaFlagsAsync(uint id)
		{
			if (id is 0)
			{
				return StatusCode(400, new ArgumentException(null, nameof(id)));
			}

			return await service.GetPlayerAsync(id) is Player player
				? StatusCode(200, new { player.Id, player.PostsBanned, player.OptedOut })
				: StatusCode(404, new ArgumentException(null, nameof(id)));
		}

		[HttpPost("Karmas")]
		public IActionResult FetchKarmas([FromBody] uint[] ids) => StatusCode(200, AccountKarmaDTO.ToDictionary(service.GetPlayersKarma(ids)));

		[HttpPost("KarmasFull")]
		public IActionResult FetchFullKarmas([FromBody] uint[] ids) => StatusCode(200, service.GetPlayersFullKarma(ids));
	}
}
