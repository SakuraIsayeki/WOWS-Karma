using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Common;

namespace WowsKarma.Web.Controllers
{
	[Route("player")]
	public class PlayerController : ControllerBase
	{
		private readonly ApiService.ApiServiceClient _apiServiceClient;

		public PlayerController(ApiService.ApiServiceClient client)
		{
			_apiServiceClient = client;
		}

		public IActionResult Index()
		{
			throw new NotImplementedException();
		}

		[Route("{id},{name}")]
		public async Task<IActionResult> Profile(uint id)
		{
			GetAccountResponse response = await _apiServiceClient.GetAccountAsync(new() { AccountId = id });
			return StatusCode(200, response.Account);
		}
	}
}
