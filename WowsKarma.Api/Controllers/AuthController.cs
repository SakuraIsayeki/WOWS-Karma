using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WowsKarma.Api.Services.Authentication;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]"), AccessKey]
	public class AuthController : ControllerBase
	{
		[HttpGet("TestAuth")]
		public IActionResult TestAuth() => StatusCode(200, "Authed request Granted");

		[HttpGet("TestAnon"), AllowAnonymous]
		public IActionResult TestAuthAnon() => StatusCode(200, "Anon request Granted.");
	}
}
