using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;



namespace WowsKarma.Web.Controllers
{
	public class PlayerController : Controller
	{
		public PlayerController() { }

		public IActionResult Index() => View();


		[Route("/{controller}/{id},{name}")]
		public async Task<IActionResult> Profile(uint id)
		{
			throw new NotImplementedException();
		}

		public async Task<IActionResult> Search(string id)
		{
			throw new NotImplementedException();
		}
	}
}
