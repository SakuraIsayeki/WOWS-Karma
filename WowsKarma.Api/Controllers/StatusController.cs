using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class StatusController : Controller
	{
		[HttpGet]
		public IActionResult Status() => Ok();
	}
}
