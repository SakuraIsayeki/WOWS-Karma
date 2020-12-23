using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Api.Services;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly WgApiFetcherService fetcher;

		public AccountController(WgApiFetcherService wgApiFetcherService)
		{
			fetcher = wgApiFetcherService;
		}

		[HttpGet("Search/{query}")]
		public async Task<IActionResult> SearchAccount(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return StatusCode(400, new ArgumentNullException(nameof(query)));
			}

			IEnumerable<AccountListingDTO> accounts = await fetcher.ListAccountsAsync(query);

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

			PlayerProfileDTO playerProfile = await fetcher.FetchAcccountAsync(id);

			return playerProfile is null
				? StatusCode(204)
				: StatusCode(200, playerProfile);
		}
	}
}
