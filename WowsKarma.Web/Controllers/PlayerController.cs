using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Common;
using WowsKarma.Web.Models;
using WowsKarma.Web.Models.ViewModels;

namespace WowsKarma.Web.Controllers
{
	public class PlayerController : Controller
	{
		private readonly ApiService.ApiServiceClient _apiServiceClient;

		public PlayerController(ApiService.ApiServiceClient client)
		{
			_apiServiceClient = client;
		}

		public IActionResult Index() => View();


		[Route("/{controller}/{id},{name}")]
		public async Task<IActionResult> Profile(uint id)
		{
			GetAccountResponse response = await _apiServiceClient.GetAccountAsync(new() { AccountId = id });
			return View(response.Account);
		}

		public async Task<IActionResult> Search(string id)
		{
			ListAccountsResponse result = await _apiServiceClient.ListAccountsAsync(new() { Search = id });
			List<PlayerListing> listings = new();

			foreach (ListAccountsResponse.Types.Result listing in result.Results)
			{
				listings.Add(new() { Id = listing.AccountId, Username = listing.Username });
			}

			return View(new SearchViewModel<PlayerListing>() { Search = id, Results = listings });
		}
	}
}
