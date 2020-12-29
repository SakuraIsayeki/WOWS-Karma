using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WowsKarma.Common.Models.DTOs;
using WowsKarma.Web.Models.ViewModels;
using WowsKarma.Web.Services;

namespace WowsKarma.Web.Controllers
{
	public class PlayerController : Controller
	{
		private readonly AccountService service;

		public PlayerController(AccountService service) 
		{
			this.service = service;
		}

		public IActionResult Index() => View();


		[Route("/{controller}/{id},{name}")]
		public async Task<IActionResult> Profile(uint id) => View(await service.FetchPlayerProfileAsync(id));

		public async Task<IActionResult> Search(string id) => View(new SearchViewModel<AccountListingDTO>()
		{
			Search = id,
			Results = (id is null || id.Length > 2) ? await service.SearchPlayersAsync(id) : null
		});
	}
}
