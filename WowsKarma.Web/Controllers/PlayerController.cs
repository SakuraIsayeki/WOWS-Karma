using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Web.Controllers
{
	[Route("player")]
	public class PlayerController : ControllerBase
	{
		public IActionResult Index()
		{
			throw new NotImplementedException();
		}

		[Route("{id},{name}")]
		public IActionResult Profile(uint id)
		{
			return StatusCode(200, $"WOWS Karma \n{id}");
		}
	}
}
