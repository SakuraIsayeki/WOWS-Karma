using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WowsKarma.Api.Services.Authentication.Wargaming;

namespace WowsKarma.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly WargamingAuthService wargamingAuthService;

		public AuthController(WargamingAuthService wargamingAuthService)
		{
			this.wargamingAuthService = wargamingAuthService;
		}

		[HttpGet("test-auth")]
		public IActionResult TestAuth() => StatusCode(200, HttpContext.User);

		[HttpGet("login")]
		public IActionResult Login() => wargamingAuthService.RedirectToLogin(Startup.ApiRegion, HttpContext.Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault()));

		[HttpGet("wg-callback")]
		public IActionResult WgAuthCallback()
		{
			return StatusCode(200);
		}
	}
}
